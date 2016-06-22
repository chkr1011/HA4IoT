using System;
using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class BedroomConfiguration : RoomConfiguration
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

            RollerShutterButtonsUpperUp,
            RollerShutterButtonsUpperDown,
            RollerShutterButtonsLowerUp,
            RollerShutterButtonsLowerDown,
            RollerShutterLeft,
            RollerShutterRight,

            Fan,

            CombinedCeilingLights,

            WindowLeft,
            WindowRight
        }

        public BedroomConfiguration(Controller controller)
            : base(controller)
        {
        }

        public override void Setup()
        {
            var hsrel5 = CCToolsBoardController.CreateHSREL5(InstalledDevice.BedroomHSREL5, new I2CSlaveAddress(38));
            var hsrel8 = CCToolsBoardController.CreateHSREL8(InstalledDevice.BedroomHSREL8, new I2CSlaveAddress(21));
            var input5 = Controller.Device<HSPE16InputOnly>(InstalledDevice.Input5);
            var input4 = Controller.Device<HSPE16InputOnly>(InstalledDevice.Input4);

            var i2cHardwareBridge = Controller.GetDevice<I2CHardwareBridge>();
            const int SensorPin = 6;

            var room = Controller.CreateArea(Room.Bedroom)
                .WithTemperatureSensor(Bedroom.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Bedroom.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
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
                .WithRollerShutter(Bedroom.RollerShutterLeft, hsrel8.GetOutput(6), hsrel8.GetOutput(5))
                .WithRollerShutter(Bedroom.RollerShutterRight, hsrel8.GetOutput(3), hsrel8.GetOutput(4))
                .WithRollerShutterButtons(Bedroom.RollerShutterButtonsUpperUp, input5.GetInput(6), Bedroom.RollerShutterButtonsUpperDown, input5.GetInput(7))
                .WithRollerShutterButtons(Bedroom.RollerShutterButtonsLowerUp, input5.GetInput(4), Bedroom.RollerShutterButtonsLowerDown, input5.GetInput(5))
                .WithWindow(Bedroom.WindowLeft, w => w.WithCenterCasement(input5.GetInput(2)))
                .WithWindow(Bedroom.WindowRight, w => w.WithCenterCasement(input5.GetInput(3)));

            room.GetRollerShutter(Bedroom.RollerShutterLeft)
                .ConnectWith(room.GetButton(Bedroom.RollerShutterButtonsUpperUp), room.GetButton(Bedroom.RollerShutterButtonsUpperDown));

            room.GetRollerShutter(Bedroom.RollerShutterRight)
                .ConnectWith(room.GetButton(Bedroom.RollerShutterButtonsLowerUp), room.GetButton(Bedroom.RollerShutterButtonsLowerDown));

            room.CombineActuators(Bedroom.CombinedCeilingLights)
                .WithActuator(room.GetLamp(Bedroom.LightCeilingWall))
                .WithActuator(room.GetLamp(Bedroom.LightCeilingWindow))
                .ConnectToggleActionWith(room.GetButton(Bedroom.ButtonDoor))
                .ConnectToggleActionWith(room.GetButton(Bedroom.ButtonWindowUpper));

            room.GetButton(Bedroom.ButtonDoor).GetPressedLongTrigger().Attach(() =>
            {
                room.GetStateMachine(Bedroom.LampBedLeft).TryTurnOff();
                room.GetStateMachine(Bedroom.LampBedRight).TryTurnOff();
                room.GetStateMachine(Bedroom.CombinedCeilingLights).TryTurnOff();
            });

            room.SetupRollerShutterAutomation()
                .WithRollerShutters(room.GetRollerShutters())
                .WithDoNotOpenBefore(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))
                .WithCloseIfOutsideTemperatureIsGreaterThan(24)
                .WithDoNotOpenIfOutsideTemperatureIsBelowThan(3);

            room.SetupTurnOnAndOffAutomation()
                .WithTrigger(room.GetMotionDetector(Bedroom.MotionDetector))
                .WithTarget(room.GetStateMachine(Bedroom.LightCeiling))
                .WithOnDuration(TimeSpan.FromSeconds(15))
                .WithTurnOnIfAllRollerShuttersClosed(room.GetRollerShutter(Bedroom.RollerShutterLeft), room.GetRollerShutter(Bedroom.RollerShutterRight))
                .WithEnabledAtNight(Controller.GetService<IDaylightService>())
                .WithSkipIfAnyActuatorIsAlreadyOn(room.GetLamp(Bedroom.LampBedLeft), room.GetLamp(Bedroom.LampBedRight));
            
            room.WithStateMachine(Bedroom.Fan, (s, r) => SetupFan(s, r, hsrel8));
            
            room.GetButton(Bedroom.ButtonBedLeftInner).WithPressedShortlyAction(() => room.GetLamp(Bedroom.LampBedLeft).SetNextState());
            room.GetButton(Bedroom.ButtonBedLeftInner).WithPressedLongAction(() => room.GetStateMachine(Bedroom.CombinedCeilingLights).SetNextState());
            room.GetButton(Bedroom.ButtonBedLeftOuter).WithPressedShortlyAction(() => room.GetStateMachine(Bedroom.Fan).SetNextState());
            room.GetButton(Bedroom.ButtonBedLeftOuter).WithPressedLongAction(() => room.GetStateMachine(Bedroom.Fan).TryTurnOff());

            room.GetButton(Bedroom.ButtonBedRightInner).WithPressedShortlyAction(() => room.GetLamp(Bedroom.LampBedRight).SetNextState());
            room.GetButton(Bedroom.ButtonBedRightInner).WithPressedLongAction(() => room.GetStateMachine(Bedroom.CombinedCeilingLights).SetNextState());
            room.GetButton(Bedroom.ButtonBedRightOuter).WithPressedShortlyAction(() => room.GetStateMachine(Bedroom.Fan).SetNextState());
            room.GetButton(Bedroom.ButtonBedRightOuter).WithPressedLongAction(() => room.GetStateMachine(Bedroom.Fan).TryTurnOff());

            Controller.GetService<SynonymService>().AddSynonymsForArea(Room.Bedroom, "Schlafzimmer", "Bedroom");
        }

        private void SetupFan(StateMachine fan, IArea room, HSREL8 hsrel8)
        {
            var fanRelay1 = hsrel8[HSREL8Pin.Relay0];
            var fanRelay2 = hsrel8[HSREL8Pin.Relay1];
            var fanRelay3 = hsrel8[HSREL8Pin.Relay2];

            fan.AddOffState()
                .WithLowOutput(fanRelay1)
                .WithLowOutput(fanRelay2)
                .WithLowOutput(fanRelay3);

            fan.AddState(new NamedComponentState("1")).WithHighOutput(fanRelay1).WithLowOutput(fanRelay2).WithHighOutput(fanRelay3);
            fan.AddState(new NamedComponentState("2")).WithHighOutput(fanRelay1).WithHighOutput(fanRelay2).WithLowOutput(fanRelay3);
            fan.AddState(new NamedComponentState("3")).WithHighOutput(fanRelay1).WithHighOutput(fanRelay2).WithHighOutput(fanRelay3);
            fan.TryTurnOff();

            fan.ConnectMoveNextAndToggleOffWith(room.GetButton(Bedroom.ButtonWindowLower));
        }
    }
}
