using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
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
            var i2cHardwareBridge = _controller.GetDevice<I2CHardwareBridge>();
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
                .WithRollerShutter(Bedroom.RollerShutterLeft, _hsrel8.GetOutput(6), _hsrel8.GetOutput(5))
                .WithRollerShutter(Bedroom.RollerShutterRight, _hsrel8.GetOutput(3), _hsrel8.GetOutput(4))
                .WithRollerShutterButtons(Bedroom.RollerShutterButtonsUpper, _input5.GetInput(6), _input5.GetInput(7))
                .WithRollerShutterButtons(Bedroom.RollerShutterButtonsLower, _input5.GetInput(4), _input5.GetInput(5))
                .WithWindow(Bedroom.WindowLeft, w => w.WithCenterCasement(_input5.GetInput(2)))
                .WithWindow(Bedroom.WindowRight, w => w.WithCenterCasement(_input5.GetInput(3)));

            bedroom.GetRollerShutter(Bedroom.RollerShutterLeft)
                .ConnectWith(bedroom.GetRollerShutterButtons(Bedroom.RollerShutterButtonsUpper));

            bedroom.GetRollerShutter(Bedroom.RollerShutterRight)
                .ConnectWith(bedroom.GetRollerShutterButtons(Bedroom.RollerShutterButtonsLower));

            bedroom.CombineActuators(Bedroom.CombinedCeilingLights)
                .WithActuator(bedroom.GetLamp(Bedroom.LightCeilingWall))
                .WithActuator(bedroom.GetLamp(Bedroom.LightCeilingWindow))
                .ConnectToggleActionWith(bedroom.GetButton(Bedroom.ButtonDoor))
                .ConnectToggleActionWith(bedroom.GetButton(Bedroom.ButtonWindowUpper));

            bedroom.GetButton(Bedroom.ButtonDoor).GetPressedLongTrigger().Attach(() =>
            {
                bedroom.GetLamp(Bedroom.LampBedLeft).TryTurnOff();
                bedroom.GetLamp(Bedroom.LampBedRight).TryTurnOff();
                bedroom.GetLamp(Bedroom.CombinedCeilingLights).TryTurnOff();
            });

            bedroom.SetupRollerShutterAutomation()
                .WithRollerShutters(bedroom.GetRollerShutters())
                .WithDoNotOpenBefore(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))
                .WithCloseIfOutsideTemperatureIsGreaterThan(24)
                .WithDoNotOpenIfOutsideTemperatureIsBelowThan(3);

            bedroom.SetupTurnOnAndOffAutomation()
                .WithTrigger(bedroom.GetMotionDetector(Bedroom.MotionDetector))
                .WithTarget(bedroom.GetStateMachine(Bedroom.LightCeiling))
                .WithOnDuration(TimeSpan.FromSeconds(15))
                .WithTurnOnIfAllRollerShuttersClosed(bedroom.GetRollerShutter(Bedroom.RollerShutterLeft), bedroom.GetRollerShutter(Bedroom.RollerShutterRight))
                .WithEnabledAtNight(_controller.GetService<IDaylightService>())
                .WithSkipIfAnyActuatorIsAlreadyOn(bedroom.GetLamp(Bedroom.LampBedLeft), bedroom.GetLamp(Bedroom.LampBedRight));
            
            bedroom.WithStateMachine(Bedroom.Fan, SetupFan);
            
            bedroom.GetButton(Bedroom.ButtonBedLeftInner).WithPressedShortlyAction(() => bedroom.GetLamp(Bedroom.LampBedLeft).SetNextState());
            bedroom.GetButton(Bedroom.ButtonBedLeftInner).WithPressedLongAction(() => bedroom.GetStateMachine(Bedroom.CombinedCeilingLights).SetNextState());
            bedroom.GetButton(Bedroom.ButtonBedLeftOuter).WithPressedShortlyAction(() => bedroom.GetStateMachine(Bedroom.Fan).SetNextState());
            bedroom.GetButton(Bedroom.ButtonBedLeftOuter).WithPressedLongAction(() => bedroom.GetStateMachine(Bedroom.Fan).TryTurnOff());

            bedroom.GetButton(Bedroom.ButtonBedRightInner).WithPressedShortlyAction(() => bedroom.GetLamp(Bedroom.LampBedRight).SetNextState());
            bedroom.GetButton(Bedroom.ButtonBedRightInner).WithPressedLongAction(() => bedroom.GetStateMachine(Bedroom.CombinedCeilingLights).SetNextState());
            bedroom.GetButton(Bedroom.ButtonBedRightOuter).WithPressedShortlyAction(() => bedroom.GetStateMachine(Bedroom.Fan).SetNextState());
            bedroom.GetButton(Bedroom.ButtonBedRightOuter).WithPressedLongAction(() => bedroom.GetStateMachine(Bedroom.Fan).TryTurnOff());
        }

        private void SetupFan(StateMachine fan, IArea room)
        {
            var fanRelay1 = _hsrel8[HSREL8Pin.Relay0];
            var fanRelay2 = _hsrel8[HSREL8Pin.Relay1];
            var fanRelay3 = _hsrel8[HSREL8Pin.Relay2];

            fan.AddOffState()
                .WithLowOutput(fanRelay1)
                .WithLowOutput(fanRelay2)
                .WithLowOutput(fanRelay3);

            fan.AddState(new StateMachineStateId("1")).WithHighOutput(fanRelay1).WithLowOutput(fanRelay2).WithHighOutput(fanRelay3);
            fan.AddState(new StateMachineStateId("2")).WithHighOutput(fanRelay1).WithHighOutput(fanRelay2).WithLowOutput(fanRelay3);
            fan.AddState(new StateMachineStateId("3")).WithHighOutput(fanRelay1).WithHighOutput(fanRelay2).WithHighOutput(fanRelay3);
            fan.TryTurnOff();

            fan.ConnectMoveNextAndToggleOffWith(room.GetButton(Bedroom.ButtonWindowLower));
        }
    }
}
