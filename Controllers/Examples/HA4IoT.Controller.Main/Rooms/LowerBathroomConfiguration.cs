using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class LowerBathroomConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly ISchedulerService _schedulerService;
        private readonly IAreaRegistryService _areaService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private TimedAction _bathmodeResetTimer;

        public enum LowerBathroom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingDoor,
            LightCeilingMiddle,
            LightCeilingWindow,
            // Another light is available!
            CombinedLights,
            CombinedLightsAutomation,

            StartBathmodeButton,
            LampMirror,

            Window
        }

        public LowerBathroomConfiguration(
            IDeviceRegistryService deviceService,
            ISchedulerService schedulerService,
            IAreaRegistryService areaService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _deviceService = deviceService;
            _schedulerService = schedulerService;
            _areaService = areaService;
            _automationFactory = automationFactory;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hspe16_FloorAndLowerBathroom = _deviceService.GetDevice<HSPE16OutputOnly>(InstalledDevice.LowerFloorAndLowerBathroomHSPE16.ToString());
            var input3 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input3.ToString());
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 3;

            var area = _areaService.RegisterArea(Room.LowerBathroom);

            _sensorFactory.RegisterWindow(area, LowerBathroom.Window, w => w.WithCenterCasement(input3.GetInput(13), input3.GetInput(14)));

            _sensorFactory.RegisterTemperatureSensor(area, LowerBathroom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, LowerBathroom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(area, LowerBathroom.MotionDetector, input3.GetInput(15));

            _sensorFactory.RegisterVirtualButton(area, LowerBathroom.StartBathmodeButton, b => b.PressedShortlyTrigger.Attach(() => StartBathode(area)));

            _actuatorFactory.RegisterLamp(area, LowerBathroom.LightCeilingDoor,
                hspe16_FloorAndLowerBathroom.GetOutput(0).WithInvertedState());

            _actuatorFactory.RegisterLamp(area, LowerBathroom.LightCeilingMiddle,
                hspe16_FloorAndLowerBathroom.GetOutput(1).WithInvertedState());

            _actuatorFactory.RegisterLamp(area, LowerBathroom.LightCeilingWindow,
                hspe16_FloorAndLowerBathroom.GetOutput(2).WithInvertedState());

            _actuatorFactory.RegisterLamp(area, LowerBathroom.LampMirror,
                hspe16_FloorAndLowerBathroom.GetOutput(4).WithInvertedState());

            _actuatorFactory.RegisterLogicalActuator(area, LowerBathroom.CombinedLights)
                .WithActuator(area.GetLamp(LowerBathroom.LightCeilingDoor))
                .WithActuator(area.GetLamp(LowerBathroom.LightCeilingMiddle))
                .WithActuator(area.GetLamp(LowerBathroom.LightCeilingWindow))
                .WithActuator(area.GetLamp(LowerBathroom.LampMirror));

            _automationFactory.RegisterTurnOnAndOffAutomation(area, LowerBathroom.CombinedLightsAutomation)
                .WithTrigger(area.GetMotionDetector(LowerBathroom.MotionDetector))
                .WithTarget(area.GetComponent(LowerBathroom.CombinedLights.ToString()));
        }

        private void StartBathode(IArea bathroom)
        {
            bathroom.GetMotionDetector().Settings.IsEnabled = false;

            bathroom.GetLamp(LowerBathroom.LightCeilingDoor).TryTurnOn();
            bathroom.GetLamp(LowerBathroom.LightCeilingMiddle).TryTurnOff();
            bathroom.GetLamp(LowerBathroom.LightCeilingWindow).TryTurnOff();
            bathroom.GetLamp(LowerBathroom.LampMirror).TryTurnOff();

            _bathmodeResetTimer?.Cancel();
            _bathmodeResetTimer = _schedulerService.In(TimeSpan.FromHours(1)).Execute(() => bathroom.GetMotionDetector().Settings.IsEnabled = true);
        }
    }
}
