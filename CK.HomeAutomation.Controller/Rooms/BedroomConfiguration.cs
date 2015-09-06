using System;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Connectors;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.Drivers;

namespace CK.HomeAutomation.Controller.Rooms
{
    internal class BedroomConfiguration
    {
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

        public void Setup(Home home, IOBoardManager ioBoardManager, CCToolsFactory ccToolsFactory, TemperatureAndHumiditySensorBridgeDriver sensorBridgeDriver)
        {
            var hsrel5 = ccToolsFactory.CreateHSREL5(Device.BedroomHSREL5, 38);
            var hsrel8 = ccToolsFactory.CreateHSREL8(Device.BedroomHSREL8, 21);
            var input5 = ioBoardManager.GetInputBoard(Device.Input5);
            var input4 = ioBoardManager.GetInputBoard(Device.Input4);

            var bedroom = home.AddRoom(Room.Bedroom)
                .WithTemperatureSensor(Bedroom.TemperatureSensor, 8, sensorBridgeDriver)
                .WithHumiditySensor(Bedroom.HumiditySensor, 8, sensorBridgeDriver)
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
                .WithRollerShutter(Bedroom.RollerShutterLeft, hsrel8.GetOutput(6), hsrel8.GetOutput(5), TimeSpan.FromSeconds(22))
                .WithRollerShutter(Bedroom.RollerShutterRight, hsrel8.GetOutput(3), hsrel8.GetOutput(4), TimeSpan.FromSeconds(22))
                .WithRollerShutterButtons(Bedroom.RollerShutterButtonsUpper, input5.GetInput(6), input5.GetInput(7))
                .WithRollerShutterButtons(Bedroom.RollerShutterButtonsLower, input5.GetInput(4), input5.GetInput(5));

            bedroom.RollerShutter(Bedroom.RollerShutterLeft)
                .ConnectWith(bedroom.RollerShutterButtons(Bedroom.RollerShutterButtonsUpper));
            bedroom.RollerShutter(Bedroom.RollerShutterRight)
                .ConnectWith(bedroom.RollerShutterButtons(Bedroom.RollerShutterButtonsLower));

            bedroom.CombineActuators(Bedroom.CombinedCeilingLights)
                .WithMaster(bedroom.Lamp(Bedroom.LightCeilingWall))
                .WithActuator(bedroom.Lamp(Bedroom.LightCeilingWindow))
                .ConnectToggleWith(bedroom.Button(Bedroom.ButtonDoor))
                .ConnectToggleWith(bedroom.Button(Bedroom.ButtonWindowUpper));

            bedroom.Button(Bedroom.ButtonDoor).WithLongAction(() =>
            {
                bedroom.Lamp(Bedroom.LampBedLeft).TurnOff();
                bedroom.Lamp(Bedroom.LampBedRight).TurnOff();
            });

            bedroom.SetupAutomaticRollerShutters()
                .WithRollerShutter(bedroom.RollerShutter(Bedroom.RollerShutterLeft))
                .WithRollerShutter(bedroom.RollerShutter(Bedroom.RollerShutterRight))
                .WithDoNotOpenBefore(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))
                .WithCloseIfOutsideTemperatureIsGreaterThan(30);

            bedroom.SetupAutomaticTurnOnAction()
                .WithMotionDetector(bedroom.MotionDetector(Bedroom.MotionDetector))
                .WithTarget(bedroom.Actuator<BinaryStateOutput>(Bedroom.LightCeiling))
                .WithOnDuration(TimeSpan.FromSeconds(15))
                .WithTimeRange(() => home.WeatherStation.Daylight.Sunset.Subtract(TimeSpan.FromHours(1)),
                    () => home.WeatherStation.Daylight.Sunrise.Add(TimeSpan.FromHours(1)));

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
    }
}
