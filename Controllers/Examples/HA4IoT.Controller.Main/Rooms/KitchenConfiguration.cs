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
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class KitchenConfiguration
    {
        private readonly IAreaRegistryService _areaService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly CCToolsBoardService _ccToolsBoardService;
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
            CombinedAutomaticLightsAutomation,

            RollerShutter,
            RollerShutterButtonUp,
            RollerShutterButtonDown,
            RollerShutterAutomation,

            ButtonPassage,
            ButtonKitchenette,

            SocketWall,
            SocketKitchenette,

            Window
        }

        public KitchenConfiguration(
            IAreaRegistryService areaService,
            IDeviceRegistryService deviceService,
            CCToolsBoardService ccToolsBoardService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _areaService = areaService;
            _deviceService = deviceService;
            _ccToolsBoardService = ccToolsBoardService;
            _automationFactory = automationFactory;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hsrel5 = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.KitchenHSREL5, new I2CSlaveAddress(58));
            var hspe8 = _ccToolsBoardService.RegisterHSPE8OutputOnly(InstalledDevice.KitchenHSPE8, new I2CSlaveAddress(39));

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

            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingPassageOuter, hspe8.GetOutput(2).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingMiddle, hsrel5.GetOutput(5).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingWindow, hsrel5.GetOutput(6).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingWall, hsrel5.GetOutput(7).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingDoor, hspe8.GetOutput(0).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Kitchen.LightCeilingPassageInner, hspe8.GetOutput(1).WithInvertedState());

            _actuatorFactory.RegisterSocket(area, Kitchen.SocketWall, hsrel5.GetOutput(2));
            _actuatorFactory.RegisterRollerShutter(area, Kitchen.RollerShutter, hsrel5.GetOutput(4), hsrel5.GetOutput(3));
            _sensorFactory.RegisterButton(area, Kitchen.ButtonKitchenette, input1.GetInput(11));
            _sensorFactory.RegisterButton(area, Kitchen.ButtonPassage, input1.GetInput(9));
            _sensorFactory.RegisterRollerShutterButtons(area, Kitchen.RollerShutterButtonUp, input2.GetInput(15),
                Kitchen.RollerShutterButtonDown, input2.GetInput(14));

            area.GetButton(Kitchen.ButtonKitchenette).PressedShortlyTrigger.Attach(() => area.GetLamp(Kitchen.LightCeilingMiddle).TryTogglePowerState());
            area.GetButton(Kitchen.ButtonPassage).PressedShortlyTrigger.Attach(() => area.GetLamp(Kitchen.LightCeilingMiddle).TryTogglePowerState());

            _automationFactory.RegisterRollerShutterAutomation(area, Kitchen.RollerShutterAutomation)
                .WithRollerShutters(area.GetRollerShutter(Kitchen.RollerShutter));

            area.GetRollerShutter(Kitchen.RollerShutter).ConnectWith(
                area.GetButton(Kitchen.RollerShutterButtonUp), area.GetButton(Kitchen.RollerShutterButtonDown));

            _actuatorFactory.RegisterLogicalComponent(area, Kitchen.CombinedAutomaticLights)
                .WithComponent(area.GetLamp(Kitchen.LightCeilingWall))
                .WithComponent(area.GetLamp(Kitchen.LightCeilingDoor))
                .WithComponent(area.GetLamp(Kitchen.LightCeilingWindow));

            _automationFactory.RegisterTurnOnAndOffAutomation(area, Kitchen.CombinedAutomaticLightsAutomation)
                .WithTrigger(area.GetMotionDetector(Kitchen.MotionDetector))
                .WithTarget(area.GetComponent(Kitchen.CombinedAutomaticLights.ToString()))
                .WithEnabledAtNight();
        }
    }
}
