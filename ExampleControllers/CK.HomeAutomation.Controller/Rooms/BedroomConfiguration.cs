using System;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Connectors;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.DHT22;
using CK.HomeAutomation.Hardware.GenericIOBoard;

namespace CK.HomeAutomation.Controller.Rooms
{
    internal class BedroomConfiguration
    {
        public void Setup(Home home, CCToolsBoardController ccToolsController, IOBoardManager ioBoardManager, DHT22Accessor dht22Accessor)
        {
            var hsrel5 = ccToolsController.CreateHSREL5(Device.BedroomHSREL5, 38);
            var hsrel8 = ccToolsController.CreateHSREL8(Device.BedroomHSREL8, 21);
            var input5 = ioBoardManager.GetInputBoard(Device.Input5);
            var input4 = ioBoardManager.GetInputBoard(Device.Input4);

            const int SensorID = 8;

            var bedroom = home.AddRoom(Room.Bedroom)
                .WithTemperatureSensor(Bedroom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorID))
                .WithHumiditySensor(Bedroom.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorID))
                .WithMotionDetector(Bedroom.MotionDetector, input5.GetInput(12))
                .WithLamp(Bedroom.LightCeiling, hsrel5.GetOutput(5).WithInvertedState())
                .WithLamp(Bedroom.LightCeilingWindow, hsrel5.GetOutput(6).WithInvertedState())
                .WithLamp(Bedroom.LightCeilingWall, hsrel5.GetOutput(7).WithInvertedState())
                .WithSocket(Bedroom.SocketWindowLeft, hsrel5.GetOutput(0))
                .WithSocket(Bedroom.SocketWindowRight, hsrel5.GetOutput(1))
                .WithSocket(Bedroom.SocketWall, hsrel5.GetOutput(2))
                .WithSocket(Bedroom.SocketWallEdge, hsrel5.GetOutput(3))
                .WithSocket(Bedroom.SocketBedLeft, hsrel8.GetOutput(7))
                .WithSocket(Bedroom.SocketBedRight, hsrel8.GetOutput(9))
                .WithLamp(Bedroom.LampBedLeft, hsrel5.GetOutput(4))
                .WithLamp(Bedroom.LampBedRight, hsrel8.GetOutput(8).WithInvertedState())
                .WithButton(Bedroom.ButtonDoor, input5.GetInput(11))
                .WithButton(Bedroom.ButtonWindowUpper, input5.GetInput(10))
                .WithButton(Bedroom.ButtonWindowLower, input5.GetInput(13))
                .WithButton(Bedroom.ButtonBedLeftInner, input4.GetInput(2))
                .WithButton(Bedroom.ButtonBedLeftOuter, input4.GetInput(0))
                .WithButton(Bedroom.ButtonBedRightInner, input4.GetInput(1))
                .WithButton(Bedroom.ButtonBedRightOuter, input4.GetInput(3))
                .WithRollerShutter(Bedroom.RollerShutterLeft, hsrel8.GetOutput(6), hsrel8.GetOutput(5), TimeSpan.FromSeconds(22), 20000)
                .WithRollerShutter(Bedroom.RollerShutterRight, hsrel8.GetOutput(3), hsrel8.GetOutput(4), TimeSpan.FromSeconds(22), 20000)
                .WithRollerShutterButtons(Bedroom.RollerShutterButtonsUpper, input5.GetInput(6), input5.GetInput(7))
                .WithRollerShutterButtons(Bedroom.RollerShutterButtonsLower, input5.GetInput(4), input5.GetInput(5));

            bedroom.RollerShutter(Bedroom.RollerShutterLeft)
                .ConnectWith(bedroom.RollerShutterButtons(Bedroom.RollerShutterButtonsUpper));
            bedroom.RollerShutter(Bedroom.RollerShutterRight)
                .ConnectWith(bedroom.RollerShutterButtons(Bedroom.RollerShutterButtonsLower));

            bedroom.CombineActuators(Bedroom.CombinedCeilingLights)
                .WithActuator(bedroom.Lamp(Bedroom.LightCeilingWall))
                .WithActuator(bedroom.Lamp(Bedroom.LightCeilingWindow))
                .ConnectToggleWith(bedroom.Button(Bedroom.ButtonDoor))
                .ConnectToggleWith(bedroom.Button(Bedroom.ButtonWindowUpper));

            bedroom.Button(Bedroom.ButtonDoor).WithLongAction(() =>
            {
                bedroom.Lamp(Bedroom.LampBedLeft).TurnOff();
                bedroom.Lamp(Bedroom.LampBedRight).TurnOff();
                bedroom.Lamp(Bedroom.CombinedCeilingLights).TurnOff();
            });

            bedroom.SetupAutomaticRollerShutters()
                .WithRollerShutter(bedroom.RollerShutter(Bedroom.RollerShutterLeft))
                .WithRollerShutter(bedroom.RollerShutter(Bedroom.RollerShutterRight))
                .WithDoNotOpenBefore(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))
                .WithCloseIfOutsideTemperatureIsGreaterThan(28);

            bedroom.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(bedroom.MotionDetector(Bedroom.MotionDetector))
                .WithTarget(bedroom.Actuator<BinaryStateOutput>(Bedroom.LightCeiling))
                .WithOnDuration(TimeSpan.FromSeconds(15))
                .WithTurnOnIfAllRollerShuttersClosed(bedroom.RollerShutter(Bedroom.RollerShutterLeft), bedroom.RollerShutter(Bedroom.RollerShutterRight))
                .WithEnabledAtNight(home.WeatherStation)
                .WithSkipIfAnyActuatorIsAlreadyOn(bedroom.Lamp(Bedroom.LampBedLeft), bedroom.Lamp(Bedroom.LampBedRight));
               
            var fanPort1 = hsrel8.GetOutput(0);
            var fanPort2 = hsrel8.GetOutput(1);
            var fanPort3 = hsrel8.GetOutput(2);
            var fan = bedroom.AddStateMachine(Bedroom.Fan);
            fan.AddOffState()
                .WithLowPort(fanPort1)
                .WithLowPort(fanPort2)
                .WithLowPort(fanPort3);
            fan.AddState("1").WithHighPort(fanPort1).WithLowPort(fanPort2).WithHighPort(fanPort3);
            fan.AddState("2").WithHighPort(fanPort1).WithHighPort(fanPort2).WithLowPort(fanPort3);
            fan.AddState("3").WithHighPort(fanPort1).WithHighPort(fanPort2).WithHighPort(fanPort3);
            fan.TurnOff();
            fan.ConnectMoveNextWith(bedroom.Button(Bedroom.ButtonWindowLower));

            bedroom.Button(Bedroom.ButtonBedLeftInner).WithShortAction(() => bedroom.Lamp(Bedroom.LampBedLeft).Toggle());
            bedroom.Button(Bedroom.ButtonBedLeftInner).WithLongAction(() => bedroom.BinaryStateOutput(Bedroom.CombinedCeilingLights).Toggle());
            bedroom.Button(Bedroom.ButtonBedLeftOuter).WithShortAction(() => bedroom.StateMachine(Bedroom.Fan).ApplyNextState());
            bedroom.Button(Bedroom.ButtonBedLeftOuter).WithLongAction(() => bedroom.StateMachine(Bedroom.Fan).TurnOff());

            bedroom.Button(Bedroom.ButtonBedRightInner).WithShortAction(() => bedroom.Lamp(Bedroom.LampBedRight).Toggle());
            bedroom.Button(Bedroom.ButtonBedRightInner).WithLongAction(() => bedroom.BinaryStateOutput(Bedroom.CombinedCeilingLights).Toggle());
            bedroom.Button(Bedroom.ButtonBedRightOuter).WithShortAction(() => bedroom.StateMachine(Bedroom.Fan).ApplyNextState());
            bedroom.Button(Bedroom.ButtonBedRightOuter).WithLongAction(() => bedroom.StateMachine(Bedroom.Fan).TurnOff());
        }

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

            CombinedCeilingLights
        }
    }
}
