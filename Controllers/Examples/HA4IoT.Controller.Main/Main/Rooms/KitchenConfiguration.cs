using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Adapters;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;
using HA4IoT.Hardware.Outpost;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal class KitchenConfiguration
    {
        private readonly ISystemEventsService _systemEventsService;
        private readonly IAreaRegistryService _areaService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly OutpostDeviceService _outpostDeviceService;
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
            LightKitchenette,
            CombinedAutomaticLights,
            CombinedAutomaticLightsAutomation,

            RollerShutter,
            RollerShutterButtonUp,
            RollerShutterButtonDown,
            RollerShutterAutomation,

            ButtonPassage,
            ButtonKitchenette,

            SocketWall,
            SocketKitchenette,

            SocketCeiling1, // Über Hängeschrank
            SocketCeiling2, // Bei Dunsabzug

            Window
        }

        public KitchenConfiguration(
            ISystemEventsService systemEventsService,
            IAreaRegistryService areaService,
            IDeviceRegistryService deviceService,
            CCToolsDeviceService ccToolsDeviceService,
            OutpostDeviceService outpostDeviceService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            _systemEventsService = systemEventsService ?? throw new ArgumentNullException(nameof(systemEventsService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _ccToolsBoardService = ccToolsDeviceService ?? throw new ArgumentNullException(nameof(ccToolsDeviceService));
            _outpostDeviceService = outpostDeviceService ?? throw new ArgumentNullException(nameof(outpostDeviceService));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
        }

        public void Apply()
        {
            var hsrel5 = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.KitchenHSREL5.ToString(), new I2CSlaveAddress(58));
            var hspe8 = _ccToolsBoardService.RegisterHSPE8OutputOnly(InstalledDevice.KitchenHSPE8.ToString(), new I2CSlaveAddress(39));

            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0.ToString());
            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1.ToString());
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input2.ToString());
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 11;

            var area = _areaService.RegisterArea(Room.Kitchen);

            _sensorFactory.RegisterWindow(area, Kitchen.Window, new PortBasedWindowAdapter(input0.GetInput(6), input0.GetInput(7)));

            _sensorFactory.RegisterTemperatureSensor(area, Kitchen.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, Kitchen.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(area, Kitchen.MotionDetector, input1.GetInput(8));

            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingMiddle, hsrel5[HSREL5Pin.GPIO0].WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingWindow, hsrel5[HSREL5Pin.GPIO1].WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingWall, hsrel5[HSREL5Pin.GPIO2].WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingDoor, hspe8[HSPE8Pin.GPIO0].WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingPassageInner, hspe8[HSPE8Pin.GPIO1].WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingPassageOuter, hspe8[HSPE8Pin.GPIO2].WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightKitchenette, _outpostDeviceService.GetRgbAdapter("RGBSK1"));

            _actuatorFactory.RegisterSocket(area, Kitchen.SocketKitchenette, hsrel5[HSREL5Pin.Relay1]); // 0?
            _actuatorFactory.RegisterSocket(area, Kitchen.SocketWall, hsrel5[HSREL5Pin.Relay2]);
            _actuatorFactory.RegisterSocket(area, Kitchen.SocketCeiling1, hspe8[HSPE8Pin.GPIO3].WithInvertedState());
            _actuatorFactory.RegisterSocket(area, Kitchen.SocketCeiling2, hspe8[HSPE8Pin.GPIO4].WithInvertedState());

            _systemEventsService.StartupCompleted += (s, e) =>
            {
                area.GetComponent(Kitchen.SocketCeiling1).TryTurnOn();
            };

            _actuatorFactory.RegisterRollerShutter(area, Kitchen.RollerShutter, hsrel5[HSREL5Pin.Relay4], hsrel5[HSREL5Pin.Relay3]);

            _sensorFactory.RegisterButton(area, Kitchen.ButtonKitchenette, input1.GetInput(11));
            _sensorFactory.RegisterButton(area, Kitchen.ButtonPassage, input1.GetInput(9));
            _sensorFactory.RegisterRollerShutterButtons(area, Kitchen.RollerShutterButtonUp, input2.GetInput(15),
                Kitchen.RollerShutterButtonDown, input2.GetInput(14));

            area.GetButton(Kitchen.ButtonKitchenette).PressedShortTrigger.Attach(() => area.GetLamp(Kitchen.LightCeilingMiddle).TryTogglePowerState());
            area.GetButton(Kitchen.ButtonPassage).PressedShortTrigger.Attach(() => area.GetLamp(Kitchen.LightCeilingMiddle).TryTogglePowerState());

            _automationFactory.RegisterRollerShutterAutomation(area, Kitchen.RollerShutterAutomation)
                .WithRollerShutters(area.GetRollerShutter(Kitchen.RollerShutter));

            area.GetRollerShutter(Kitchen.RollerShutter).ConnectWith(
                area.GetButton(Kitchen.RollerShutterButtonUp), area.GetButton(Kitchen.RollerShutterButtonDown));

            area.GetButton(Kitchen.RollerShutterButtonUp).PressedLongTrigger.Attach(() =>
            {
                var light = area.GetComponent(Kitchen.LightKitchenette);
                light.TryTogglePowerState();
                light.TrySetColor(0D, 0D, 1D);
            });

            _actuatorFactory.RegisterLogicalComponent(area, Kitchen.CombinedAutomaticLights)
                .WithComponent(area.GetLamp(Kitchen.LightCeilingWall))
                .WithComponent(area.GetLamp(Kitchen.LightCeilingDoor))
                .WithComponent(area.GetLamp(Kitchen.LightCeilingWindow));

            _automationFactory.RegisterTurnOnAndOffAutomation(area, Kitchen.CombinedAutomaticLightsAutomation)
                .WithTrigger(area.GetMotionDetector(Kitchen.MotionDetector))
                .WithTarget(area.GetComponent(Kitchen.CombinedAutomaticLights))
                .WithEnabledAtNight();
        }
    }
}
