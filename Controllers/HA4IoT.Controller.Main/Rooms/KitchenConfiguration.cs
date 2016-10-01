using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class KitchenConfiguration
    {
        private readonly IAreaService _areaService;
        private readonly IDeviceService _deviceService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly SynonymService _synonymService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;

        public enum Kitchen
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingMiddle,
            LightCeilingWall,
            LightCeilingWindow,
            LightCeilingDoor,
            LightCeilingPassageOuter,
            LightCeilingPassageInner,
            CombinedAutomaticLights,

            RollerShutter,
            RollerShutterButtonUp,
            RollerShutterButtonDown,

            ButtonPassage,
            ButtonKitchenette,

            SocketWall,
            SocketKitchenette,

            Window
        }

        public KitchenConfiguration(
            IAreaService areaService,
            IDeviceService deviceService,
            CCToolsBoardService ccToolsBoardService,
            SynonymService synonymService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _areaService = areaService;
            _deviceService = deviceService;
            _ccToolsBoardService = ccToolsBoardService;
            _synonymService = synonymService;
            _automationFactory = automationFactory;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hsrel5 = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.KitchenHSREL5, new I2CSlaveAddress(58));
            var hspe8 = _ccToolsBoardService.RegisterHSPE8OutputOnly(InstalledDevice.KitchenHSPE8, new I2CSlaveAddress(39));

            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0);
            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1);
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input2);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 11;

            var room = _areaService.CreateArea(Room.Kitchen);

            _sensorFactory.RegisterWindow(room, Kitchen.Window, w => w.WithCenterCasement(input0.GetInput(6), input0.GetInput(7)));

            _sensorFactory.RegisterTemperatureSensor(room, Kitchen.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(room, Kitchen.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(room, Kitchen.MotionDetector, input1.GetInput(8));

            _actuatorFactory.RegisterLamp(room, Kitchen.LightCeilingPassageOuter, hspe8.GetOutput(2).WithInvertedState());
            _actuatorFactory.RegisterLamp(room, Kitchen.LightCeilingMiddle, hsrel5.GetOutput(5).WithInvertedState());
            _actuatorFactory.RegisterLamp(room, Kitchen.LightCeilingWindow, hsrel5.GetOutput(6).WithInvertedState());
            _actuatorFactory.RegisterLamp(room, Kitchen.LightCeilingWall, hsrel5.GetOutput(7).WithInvertedState());
            _actuatorFactory.RegisterLamp(room, Kitchen.LightCeilingDoor, hspe8.GetOutput(0).WithInvertedState());
            _actuatorFactory.RegisterLamp(room, Kitchen.LightCeilingPassageInner, hspe8.GetOutput(1).WithInvertedState());

            _actuatorFactory.RegisterSocket(room, Kitchen.SocketWall, hsrel5.GetOutput(2));
            _actuatorFactory.RegisterRollerShutter(room, Kitchen.RollerShutter, hsrel5.GetOutput(4), hsrel5.GetOutput(3));
            _sensorFactory.RegisterButton(room, Kitchen.ButtonKitchenette, input1.GetInput(11));
            _sensorFactory.RegisterButton(room, Kitchen.ButtonPassage, input1.GetInput(9));
            _sensorFactory.RegisterRollerShutterButtons(room, Kitchen.RollerShutterButtonUp, input2.GetInput(15),
                Kitchen.RollerShutterButtonDown, input2.GetInput(14));

            room.GetLamp(Kitchen.LightCeilingMiddle).ConnectToggleActionWith(room.GetButton(Kitchen.ButtonKitchenette));
            room.GetLamp(Kitchen.LightCeilingMiddle).ConnectToggleActionWith(room.GetButton(Kitchen.ButtonPassage));

            _automationFactory.RegisterRollerShutterAutomation(room)
                .WithRollerShutters(room.GetRollerShutter(Kitchen.RollerShutter));

            room.GetRollerShutter(Kitchen.RollerShutter).ConnectWith(
                room.GetButton(Kitchen.RollerShutterButtonUp), room.GetButton(Kitchen.RollerShutterButtonDown));

            _actuatorFactory.RegisterLogicalActuator(room, Kitchen.CombinedAutomaticLights)
                .WithActuator(room.GetLamp(Kitchen.LightCeilingWall))
                .WithActuator(room.GetLamp(Kitchen.LightCeilingDoor))
                .WithActuator(room.GetLamp(Kitchen.LightCeilingWindow));

            _automationFactory.RegisterTurnOnAndOffAutomation(room)
                .WithTrigger(room.GetMotionDetector(Kitchen.MotionDetector))
                .WithTarget(room.GetActuator(Kitchen.CombinedAutomaticLights))
                .WithEnabledAtNight();

            _synonymService.AddSynonymsForArea(Room.Kitchen, "Küche", "Kitchen");
        }
    }
}
