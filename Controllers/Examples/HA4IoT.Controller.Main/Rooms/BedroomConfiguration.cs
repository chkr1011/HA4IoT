using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class BedroomConfiguration
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaRespositoryService _areaService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly AutomationFactory _automationFactory;

        private enum Bedroom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeiling,
            LightCeilingAutomation,
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
            RollerShuttersAutomation,

            Fan,

            CombinedCeilingLights,

            WindowLeft,
            WindowRight
        }

        public BedroomConfiguration(
            IDeviceService deviceService,
            IAreaRespositoryService areaService,
            CCToolsBoardService ccToolsBoardService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            AutomationFactory automationFactory)
        {
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));

            _deviceService = deviceService;
            _areaService = areaService;
            _ccToolsBoardService = ccToolsBoardService;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
            _automationFactory = automationFactory;
        }

        public void Apply()
        {
            var hsrel5 = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.BedroomHSREL5, new I2CSlaveAddress(38));
            var hsrel8 = _ccToolsBoardService.RegisterHSREL8(InstalledDevice.BedroomHSREL8, new I2CSlaveAddress(21));
            var input5 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input5);
            var input4 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input4);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 6;

            var area = _areaService.CreateArea(Room.Bedroom);

            _sensorFactory.RegisterWindow(area, Bedroom.WindowLeft, w => w.WithCenterCasement(input5.GetInput(2)));
            _sensorFactory.RegisterWindow(area, Bedroom.WindowRight, w => w.WithCenterCasement(input5.GetInput(3)));

            _sensorFactory.RegisterTemperatureSensor(area, Bedroom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, Bedroom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(area, Bedroom.MotionDetector, input5.GetInput(12));

            _sensorFactory.RegisterButton(area, Bedroom.ButtonWindowUpper, input5.GetInput(10));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonWindowLower, input5.GetInput(13));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonBedLeftInner, input4.GetInput(2));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonBedLeftOuter, input4.GetInput(0));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonBedRightInner, input4.GetInput(1));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonBedRightOuter, input4.GetInput(3));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonDoor, input5.GetInput(11));

            _sensorFactory.RegisterRollerShutterButtons(area, Bedroom.RollerShutterButtonsUpperUp, input5.GetInput(6),
                Bedroom.RollerShutterButtonsUpperDown, input5.GetInput(7));

            _sensorFactory.RegisterRollerShutterButtons(area, Bedroom.RollerShutterButtonsLowerUp, input5.GetInput(4),
                Bedroom.RollerShutterButtonsLowerDown, input5.GetInput(5));

            _actuatorFactory.RegisterLamp(area, Bedroom.LightCeiling, hsrel5.GetOutput(5).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Bedroom.LightCeilingWindow, hsrel5.GetOutput(6).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Bedroom.LightCeilingWall, hsrel5.GetOutput(7).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Bedroom.LampBedLeft, hsrel5.GetOutput(4));
            _actuatorFactory.RegisterLamp(area, Bedroom.LampBedRight, hsrel8.GetOutput(8).WithInvertedState());

            _actuatorFactory.RegisterSocket(area, Bedroom.SocketWindowLeft, hsrel5[HSREL5Pin.Relay0]);
            _actuatorFactory.RegisterSocket(area, Bedroom.SocketWindowRight, hsrel5[HSREL5Pin.Relay1]);
            _actuatorFactory.RegisterSocket(area, Bedroom.SocketWall, hsrel5[HSREL5Pin.Relay2]);
            _actuatorFactory.RegisterSocket(area, Bedroom.SocketWallEdge, hsrel5[HSREL5Pin.Relay3]);
            _actuatorFactory.RegisterSocket(area, Bedroom.SocketBedLeft, hsrel8.GetOutput(7));
            _actuatorFactory.RegisterSocket(area, Bedroom.SocketBedRight, hsrel8.GetOutput(9));

            _actuatorFactory.RegisterRollerShutter(area, Bedroom.RollerShutterLeft, hsrel8[HSREL8Pin.Relay6], hsrel8[HSREL8Pin.Relay5]);
            _actuatorFactory.RegisterRollerShutter(area, Bedroom.RollerShutterRight, hsrel8[HSREL8Pin.Relay3], hsrel8[HSREL8Pin.Relay4]);

            area.GetRollerShutter(Bedroom.RollerShutterLeft)
                .ConnectWith(area.GetButton(Bedroom.RollerShutterButtonsUpperUp), area.GetButton(Bedroom.RollerShutterButtonsUpperDown));

            area.GetRollerShutter(Bedroom.RollerShutterRight)
                .ConnectWith(area.GetButton(Bedroom.RollerShutterButtonsLowerUp), area.GetButton(Bedroom.RollerShutterButtonsLowerDown));

            _actuatorFactory.RegisterLogicalActuator(area, Bedroom.CombinedCeilingLights)
                .WithActuator(area.GetLamp(Bedroom.LightCeilingWall))
                .WithActuator(area.GetLamp(Bedroom.LightCeilingWindow))
                .ConnectToggleActionWith(area.GetButton(Bedroom.ButtonDoor))
                .ConnectToggleActionWith(area.GetButton(Bedroom.ButtonWindowUpper));

            area.GetButton(Bedroom.ButtonDoor).PressedLongTrigger.Attach(() =>
            {
                area.GetStateMachine(Bedroom.LampBedLeft).TryTurnOff();
                area.GetStateMachine(Bedroom.LampBedRight).TryTurnOff();
                area.GetStateMachine(Bedroom.CombinedCeilingLights).TryTurnOff();
            });

            _automationFactory.RegisterRollerShutterAutomation(area, Bedroom.RollerShuttersAutomation)
                .WithRollerShutters(area.GetRollerShutters())
                .WithDoNotOpenBefore(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))
                .WithCloseIfOutsideTemperatureIsGreaterThan(24)
                .WithDoNotOpenIfOutsideTemperatureIsBelowThan(3);

            _automationFactory.RegisterTurnOnAndOffAutomation(area, Bedroom.LightCeilingAutomation)
                .WithTrigger(area.GetMotionDetector(Bedroom.MotionDetector))
                .WithTarget(area.GetStateMachine(Bedroom.LightCeiling))
                .WithTurnOnIfAllRollerShuttersClosed(area.GetRollerShutter(Bedroom.RollerShutterLeft), area.GetRollerShutter(Bedroom.RollerShutterRight))
                .WithEnabledAtNight()
                .WithSkipIfAnyActuatorIsAlreadyOn(area.GetLamp(Bedroom.LampBedLeft), area.GetLamp(Bedroom.LampBedRight));

            _actuatorFactory.RegisterStateMachine(area, Bedroom.Fan, (s, r) => SetupFan(s, r, hsrel8));

            area.GetButton(Bedroom.ButtonBedLeftInner).WithPressedShortlyAction(() => area.GetLamp(Bedroom.LampBedLeft).SetNextState());
            area.GetButton(Bedroom.ButtonBedLeftInner).WithPressedLongAction(() => area.GetStateMachine(Bedroom.CombinedCeilingLights).SetNextState());
            area.GetButton(Bedroom.ButtonBedLeftOuter).WithPressedShortlyAction(() => area.GetStateMachine(Bedroom.Fan).SetNextState());
            area.GetButton(Bedroom.ButtonBedLeftOuter).WithPressedLongAction(() => area.GetStateMachine(Bedroom.Fan).TryTurnOff());

            area.GetButton(Bedroom.ButtonBedRightInner).WithPressedShortlyAction(() => area.GetLamp(Bedroom.LampBedRight).SetNextState());
            area.GetButton(Bedroom.ButtonBedRightInner).WithPressedLongAction(() => area.GetStateMachine(Bedroom.CombinedCeilingLights).SetNextState());
            area.GetButton(Bedroom.ButtonBedRightOuter).WithPressedShortlyAction(() => area.GetStateMachine(Bedroom.Fan).SetNextState());
            area.GetButton(Bedroom.ButtonBedRightOuter).WithPressedLongAction(() => area.GetStateMachine(Bedroom.Fan).TryTurnOff());
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

            fan.AddState(new ComponentState("1")).WithHighOutput(fanRelay1).WithLowOutput(fanRelay2).WithHighOutput(fanRelay3);
            fan.AddState(new ComponentState("2")).WithHighOutput(fanRelay1).WithHighOutput(fanRelay2).WithLowOutput(fanRelay3);
            fan.AddState(new ComponentState("3")).WithHighOutput(fanRelay1).WithHighOutput(fanRelay2).WithHighOutput(fanRelay3);
            fan.TryTurnOff();

            fan.ConnectMoveNextAndToggleOffWith(room.GetButton(Bedroom.ButtonWindowLower));
        }
    }
}
