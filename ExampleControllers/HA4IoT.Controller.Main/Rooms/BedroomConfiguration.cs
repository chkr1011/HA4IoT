using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class BedroomConfiguration
    {
        private readonly Controller _controller;
        private readonly HSREL5 _hsrel5;
        private readonly HSREL8 _hsrel8;
        private readonly IBinaryInputController _input5;
        private readonly IBinaryInputController _input4;

        private enum Bedroom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeiling,
            LightCeilingWindow,
            LightCeilingWall,

            LampBedLeft,
            LampBedRight,

            SocketWindowLeft,
            SocketWindowRight,
            SocketWall,
            SocketWallEdge,
            SocketBedLeft,
            SocketBedRight,

            ButtonDoor,
            ButtonWindowUpper,
            ButtonWindowLower,

            ButtonBedLeftInner,
            ButtonBedLeftOuter,
            ButtonBedRightInner,
            ButtonBedRightOuter,

            RollerShutterButtonsUpper,
            RollerShutterButtonsLower,
            RollerShutterLeft,
            RollerShutterRight,

            Fan,

            CombinedCeilingLights,

            WindowLeft,
            WindowRight
        }

        public BedroomConfiguration(Controller controller, CCToolsBoardController ccToolsController)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (ccToolsController == null) throw new ArgumentNullException(nameof(ccToolsController));

            _controller = controller;

            _hsrel5 = ccToolsController.CreateHSREL5(Device.BedroomHSREL5, new I2CSlaveAddress(38));
            _hsrel8 = ccToolsController.CreateHSREL8(Device.BedroomHSREL8, new I2CSlaveAddress(21));
            _input5 = controller.Device<HSPE16InputOnly>(Device.Input5);
            _input4 = controller.Device<HSPE16InputOnly>(Device.Input4);
        }

        public void Setup()
        {
            var i2cHardwareBridge = _controller.Device<I2CHardwareBridge>();
            const int SensorPin = 6;

            var bedroom = _controller.CreateArea(Room.Bedroom)
                .WithTemperatureSensor(Bedroom.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Bedroom.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithMotionDetector(Bedroom.MotionDetector, _input5.GetInput(12))
                .WithLamp(Bedroom.LightCeiling, _hsrel5.GetOutput(5).WithInvertedState())
                .WithLamp(Bedroom.LightCeilingWindow, _hsrel5.GetOutput(6).WithInvertedState())
                .WithLamp(Bedroom.LightCeilingWall, _hsrel5.GetOutput(7).WithInvertedState())
                .WithSocket(Bedroom.SocketWindowLeft, _hsrel5.GetOutput(0))
                .WithSocket(Bedroom.SocketWindowRight, _hsrel5.GetOutput(1))
                .WithSocket(Bedroom.SocketWall, _hsrel5.GetOutput(2))
                .WithSocket(Bedroom.SocketWallEdge, _hsrel5.GetOutput(3))
                .WithSocket(Bedroom.SocketBedLeft, _hsrel8.GetOutput(7))
                .WithSocket(Bedroom.SocketBedRight, _hsrel8.GetOutput(9))
                .WithLamp(Bedroom.LampBedLeft, _hsrel5.GetOutput(4))
                .WithLamp(Bedroom.LampBedRight, _hsrel8.GetOutput(8).WithInvertedState())
                .WithButton(Bedroom.ButtonDoor, _input5.GetInput(11))
                .WithButton(Bedroom.ButtonWindowUpper, _input5.GetInput(10))
                .WithButton(Bedroom.ButtonWindowLower, _input5.GetInput(13))
                .WithButton(Bedroom.ButtonBedLeftInner, _input4.GetInput(2))
                .WithButton(Bedroom.ButtonBedLeftOuter, _input4.GetInput(0))
                .WithButton(Bedroom.ButtonBedRightInner, _input4.GetInput(1))
                .WithButton(Bedroom.ButtonBedRightOuter, _input4.GetInput(3))
                .WithRollerShutter(Bedroom.RollerShutterLeft, _hsrel8.GetOutput(6), _hsrel8.GetOutput(5), TimeSpan.FromSeconds(20))
                .WithRollerShutter(Bedroom.RollerShutterRight, _hsrel8.GetOutput(3), _hsrel8.GetOutput(4), TimeSpan.FromSeconds(20))
                .WithRollerShutterButtons(Bedroom.RollerShutterButtonsUpper, _input5.GetInput(6), _input5.GetInput(7))
                .WithRollerShutterButtons(Bedroom.RollerShutterButtonsLower, _input5.GetInput(4), _input5.GetInput(5))
                .WithWindow(Bedroom.WindowLeft, w => w.WithCenterCasement(_input5.GetInput(2)))
                .WithWindow(Bedroom.WindowRight, w => w.WithCenterCasement(_input5.GetInput(3)));

            bedroom.RollerShutter(Bedroom.RollerShutterLeft)
                .ConnectWith(bedroom.RollerShutterButtons(Bedroom.RollerShutterButtonsUpper));
            bedroom.RollerShutter(Bedroom.RollerShutterRight)
                .ConnectWith(bedroom.RollerShutterButtons(Bedroom.RollerShutterButtonsLower));

            bedroom.CombineActuators(Bedroom.CombinedCeilingLights)
                .WithActuator(bedroom.Lamp(Bedroom.LightCeilingWall))
                .WithActuator(bedroom.Lamp(Bedroom.LightCeilingWindow))
                .ConnectToggleActionWith(bedroom.Button(Bedroom.ButtonDoor))
                .ConnectToggleActionWith(bedroom.Button(Bedroom.ButtonWindowUpper));

            bedroom.Button(Bedroom.ButtonDoor).WithLongAction(() =>
            {
                bedroom.Lamp(Bedroom.LampBedLeft).TurnOff();
                bedroom.Lamp(Bedroom.LampBedRight).TurnOff();
                bedroom.Lamp(Bedroom.CombinedCeilingLights).TurnOff();
            });

            bedroom.SetupRollerShutterAutomation()
                .WithRollerShutters(bedroom.GetAllRollerShutters())
                .WithDoNotOpenBefore(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))
                .WithCloseIfOutsideTemperatureIsGreaterThan(24)
                .WithDoNotOpenIfOutsideTemperatureIsBelowThan(3);

            bedroom.SetupTurnOnAndOffAutomation()
                .WithTrigger(bedroom.MotionDetector(Bedroom.MotionDetector))
                .WithTarget(bedroom.BinaryStateOutput(Bedroom.LightCeiling))
                .WithOnDuration(TimeSpan.FromSeconds(15))
                .WithTurnOnIfAllRollerShuttersClosed(bedroom.RollerShutter(Bedroom.RollerShutterLeft), bedroom.RollerShutter(Bedroom.RollerShutterRight))
                .WithEnabledAtNight(_controller.Device<IWeatherStation>())
                .WithSkipIfAnyActuatorIsAlreadyOn(bedroom.Lamp(Bedroom.LampBedLeft), bedroom.Lamp(Bedroom.LampBedRight));
            
            bedroom.WithStateMachine(Bedroom.Fan, SetupFan);
            
            bedroom.Button(Bedroom.ButtonBedLeftInner).WithShortAction(() => bedroom.Lamp(Bedroom.LampBedLeft).Toggle());
            bedroom.Button(Bedroom.ButtonBedLeftInner).WithLongAction(() => bedroom.BinaryStateOutput(Bedroom.CombinedCeilingLights).Toggle());
            bedroom.Button(Bedroom.ButtonBedLeftOuter).WithShortAction(() => bedroom.StateMachine(Bedroom.Fan).SetNextState());
            bedroom.Button(Bedroom.ButtonBedLeftOuter).WithLongAction(() => bedroom.StateMachine(Bedroom.Fan).TurnOff());

            bedroom.Button(Bedroom.ButtonBedRightInner).WithShortAction(() => bedroom.Lamp(Bedroom.LampBedRight).Toggle());
            bedroom.Button(Bedroom.ButtonBedRightInner).WithLongAction(() => bedroom.BinaryStateOutput(Bedroom.CombinedCeilingLights).Toggle());
            bedroom.Button(Bedroom.ButtonBedRightOuter).WithShortAction(() => bedroom.StateMachine(Bedroom.Fan).SetNextState());
            bedroom.Button(Bedroom.ButtonBedRightOuter).WithLongAction(() => bedroom.StateMachine(Bedroom.Fan).TurnOff());
        }

        private void SetupFan(StateMachine fan, IArea room)
        {
            var fanRelay1 = _hsrel8[HSREL8Pin.Relay0];
            var fanRelay2 = _hsrel8[HSREL8Pin.Relay1];
            var fanRelay3 = _hsrel8[HSREL8Pin.Relay2];

            fan.AddOffState()
                .WithLowPort(fanRelay1)
                .WithLowPort(fanRelay2)
                .WithLowPort(fanRelay3);

            fan.AddState("1").WithHighPort(fanRelay1).WithLowPort(fanRelay2).WithHighPort(fanRelay3);
            fan.AddState("2").WithHighPort(fanRelay1).WithHighPort(fanRelay2).WithLowPort(fanRelay3);
            fan.AddState("3").WithHighPort(fanRelay1).WithHighPort(fanRelay2).WithHighPort(fanRelay3);
            fan.TurnOff();

            fan.ConnectMoveNextAndToggleOffWith(room.Button(Bedroom.ButtonWindowLower));
        }
    }
}
