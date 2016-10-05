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
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class BedroomConfiguration
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly SynonymService _synonymService;
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
            IAreaService areaService,
            CCToolsBoardService ccToolsBoardService,
            SynonymService synonymService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            AutomationFactory automationFactory)
        {
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));

            _deviceService = deviceService;
            _areaService = areaService;
            _ccToolsBoardService = ccToolsBoardService;
            _synonymService = synonymService;
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

            var room = _areaService.CreateArea(Room.Bedroom);

            _sensorFactory.RegisterWindow(room, Bedroom.WindowLeft, w => w.WithCenterCasement(input5.GetInput(2)));
            _sensorFactory.RegisterWindow(room, Bedroom.WindowRight, w => w.WithCenterCasement(input5.GetInput(3)));

            _sensorFactory.RegisterTemperatureSensor(room, Bedroom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(room, Bedroom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(room, Bedroom.MotionDetector, input5.GetInput(12));

            _sensorFactory.RegisterButton(room, Bedroom.ButtonWindowUpper, input5.GetInput(10));
            _sensorFactory.RegisterButton(room, Bedroom.ButtonWindowLower, input5.GetInput(13));
            _sensorFactory.RegisterButton(room, Bedroom.ButtonBedLeftInner, input4.GetInput(2));
            _sensorFactory.RegisterButton(room, Bedroom.ButtonBedLeftOuter, input4.GetInput(0));
            _sensorFactory.RegisterButton(room, Bedroom.ButtonBedRightInner, input4.GetInput(1));
            _sensorFactory.RegisterButton(room, Bedroom.ButtonBedRightOuter, input4.GetInput(3));
            _sensorFactory.RegisterButton(room, Bedroom.ButtonDoor, input5.GetInput(11));

            _sensorFactory.RegisterRollerShutterButtons(room, Bedroom.RollerShutterButtonsUpperUp, input5.GetInput(6),
                Bedroom.RollerShutterButtonsUpperDown, input5.GetInput(7));

            _sensorFactory.RegisterRollerShutterButtons(room, Bedroom.RollerShutterButtonsLowerUp, input5.GetInput(4),
                Bedroom.RollerShutterButtonsLowerDown, input5.GetInput(5));

            _actuatorFactory.RegisterLamp(room, Bedroom.LightCeiling, hsrel5.GetOutput(5).WithInvertedState());
            _actuatorFactory.RegisterLamp(room, Bedroom.LightCeilingWindow, hsrel5.GetOutput(6).WithInvertedState());
            _actuatorFactory.RegisterLamp(room, Bedroom.LightCeilingWall, hsrel5.GetOutput(7).WithInvertedState());
            _actuatorFactory.RegisterLamp(room, Bedroom.LampBedLeft, hsrel5.GetOutput(4));
            _actuatorFactory.RegisterLamp(room, Bedroom.LampBedRight, hsrel8.GetOutput(8).WithInvertedState());

            _actuatorFactory.RegisterSocket(room, Bedroom.SocketWindowLeft, hsrel5[HSREL5Pin.Relay0]);
            _actuatorFactory.RegisterSocket(room, Bedroom.SocketWindowRight, hsrel5[HSREL5Pin.Relay1]);
            _actuatorFactory.RegisterSocket(room, Bedroom.SocketWall, hsrel5[HSREL5Pin.Relay2]);
            _actuatorFactory.RegisterSocket(room, Bedroom.SocketWallEdge, hsrel5[HSREL5Pin.Relay3]);
            _actuatorFactory.RegisterSocket(room, Bedroom.SocketBedLeft, hsrel8.GetOutput(7));
            _actuatorFactory.RegisterSocket(room, Bedroom.SocketBedRight, hsrel8.GetOutput(9));

            _actuatorFactory.RegisterRollerShutter(room, Bedroom.RollerShutterLeft, hsrel8[HSREL8Pin.Relay6], hsrel8[HSREL8Pin.Relay5]);
            _actuatorFactory.RegisterRollerShutter(room, Bedroom.RollerShutterRight, hsrel8[HSREL8Pin.Relay3], hsrel8[HSREL8Pin.Relay4]);

            room.GetRollerShutter(Bedroom.RollerShutterLeft)
                .ConnectWith(room.GetButton(Bedroom.RollerShutterButtonsUpperUp), room.GetButton(Bedroom.RollerShutterButtonsUpperDown));

            room.GetRollerShutter(Bedroom.RollerShutterRight)
                .ConnectWith(room.GetButton(Bedroom.RollerShutterButtonsLowerUp), room.GetButton(Bedroom.RollerShutterButtonsLowerDown));

            _actuatorFactory.RegisterLogicalActuator(room, Bedroom.CombinedCeilingLights)
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

            _automationFactory.RegisterRollerShutterAutomation(room, Bedroom.RollerShuttersAutomation)
                .WithRollerShutters(room.GetRollerShutters())
                .WithDoNotOpenBefore(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))
                .WithCloseIfOutsideTemperatureIsGreaterThan(24)
                .WithDoNotOpenIfOutsideTemperatureIsBelowThan(3);

            _automationFactory.RegisterTurnOnAndOffAutomation(room, Bedroom.LightCeilingAutomation)
                .WithTrigger(room.GetMotionDetector(Bedroom.MotionDetector))
                .WithTarget(room.GetStateMachine(Bedroom.LightCeiling))
                .WithTurnOnIfAllRollerShuttersClosed(room.GetRollerShutter(Bedroom.RollerShutterLeft), room.GetRollerShutter(Bedroom.RollerShutterRight))
                .WithEnabledAtNight()
                .WithSkipIfAnyActuatorIsAlreadyOn(room.GetLamp(Bedroom.LampBedLeft), room.GetLamp(Bedroom.LampBedRight));

            _actuatorFactory.RegisterStateMachine(room, Bedroom.Fan, (s, r) => SetupFan(s, r, hsrel8));

            room.GetButton(Bedroom.ButtonBedLeftInner).WithPressedShortlyAction(() => room.GetLamp(Bedroom.LampBedLeft).SetNextState());
            room.GetButton(Bedroom.ButtonBedLeftInner).WithPressedLongAction(() => room.GetStateMachine(Bedroom.CombinedCeilingLights).SetNextState());
            room.GetButton(Bedroom.ButtonBedLeftOuter).WithPressedShortlyAction(() => room.GetStateMachine(Bedroom.Fan).SetNextState());
            room.GetButton(Bedroom.ButtonBedLeftOuter).WithPressedLongAction(() => room.GetStateMachine(Bedroom.Fan).TryTurnOff());

            room.GetButton(Bedroom.ButtonBedRightInner).WithPressedShortlyAction(() => room.GetLamp(Bedroom.LampBedRight).SetNextState());
            room.GetButton(Bedroom.ButtonBedRightInner).WithPressedLongAction(() => room.GetStateMachine(Bedroom.CombinedCeilingLights).SetNextState());
            room.GetButton(Bedroom.ButtonBedRightOuter).WithPressedShortlyAction(() => room.GetStateMachine(Bedroom.Fan).SetNextState());
            room.GetButton(Bedroom.ButtonBedRightOuter).WithPressedLongAction(() => room.GetStateMachine(Bedroom.Fan).TryTurnOff());

            _synonymService.AddSynonymsForArea(Room.Bedroom, "Schlafzimmer", "Bedroom");
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
