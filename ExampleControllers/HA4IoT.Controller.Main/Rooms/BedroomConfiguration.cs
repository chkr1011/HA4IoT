using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.DHT22;
using HA4IoT.Hardware.GenericIOBoard;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class BedroomConfiguration
    {
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

        public BedroomConfiguration(CCToolsBoardController ccToolsController, IOBoardCollection ioBoardCollection)
        {
            _hsrel5 = ccToolsController.CreateHSREL5(Device.BedroomHSREL5, new I2CSlaveAddress(38));
            _hsrel8 = ccToolsController.CreateHSREL8(Device.BedroomHSREL8, new I2CSlaveAddress(21));
            _input5 = ioBoardCollection.GetInputBoard(Device.Input5);
            _input4 = ioBoardCollection.GetInputBoard(Device.Input4);
        }

        public void Setup(Home home, DHT22Accessor dht22Accessor)
        {
            const int SensorPin = 6;

            var bedroom = home.AddRoom(Room.Bedroom)
                .WithTemperatureSensor(Bedroom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Bedroom.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorPin))
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
                .WithRollerShutter(Bedroom.RollerShutterLeft, _hsrel8.GetOutput(6), _hsrel8.GetOutput(5), TimeSpan.FromSeconds(20), 17000)
                .WithRollerShutter(Bedroom.RollerShutterRight, _hsrel8.GetOutput(3), _hsrel8.GetOutput(4), TimeSpan.FromSeconds(20), 17000)
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

            bedroom.SetupAutomaticRollerShutters()
                .WithRollerShutters(bedroom.GetAllRollerShutters())
                .WithDoNotOpenBefore(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))
                .WithCloseIfOutsideTemperatureIsGreaterThan(24)
                .WithDoNotOpenIfOutsideTemperatureIsBelowThan(3);

            bedroom.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(bedroom.MotionDetector(Bedroom.MotionDetector))
                .WithTarget(bedroom.Actuator<BinaryStateOutputActuator>(Bedroom.LightCeiling))
                .WithOnDuration(TimeSpan.FromSeconds(15))
                .WithTurnOnIfAllRollerShuttersClosed(bedroom.RollerShutter(Bedroom.RollerShutterLeft), bedroom.RollerShutter(Bedroom.RollerShutterRight))
                .WithEnabledAtNight(home.WeatherStation)
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

        private void SetupFan(StateMachine fan, Actuators.Room room)
        {
            var fanRelay1 = _hsrel8[HSREL8Output.Relay0];
            var fanRelay2 = _hsrel8[HSREL8Output.Relay1];
            var fanRelay3 = _hsrel8[HSREL8Output.Relay2];

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
