using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Fans;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Automations;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal class UpperBathroomConfiguration
    {
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly ISchedulerService _schedulerService;
        private readonly IAreaRegistryService _areaService;
        private readonly ISettingsService _settingsService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;

        private enum UpperBathroom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingDoor,
            LightCeilingEdge,
            LightCeilingMirrorCabinet,
            LampMirrorCabinet,

            Fan,
            FanAutomation,

            CombinedCeilingLights,
            CombinedCeilingLightsAutomation
        }

        public UpperBathroomConfiguration(
            CCToolsBoardService ccToolsBoardService,
            IDeviceRegistryService deviceService,
            ISchedulerService schedulerService,
            IAreaRegistryService areaService,
            ISettingsService settingsService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _ccToolsBoardService = ccToolsBoardService;
            _deviceService = deviceService;
            _schedulerService = schedulerService;
            _areaService = areaService;
            _settingsService = settingsService;
            _automationFactory = automationFactory;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hsrel5 = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.UpperBathroomHSREL5, new I2CSlaveAddress(61));
            var input5 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input5.ToString());
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 4;

            var area = _areaService.RegisterArea(Room.UpperBathroom);

            area.AddComponent(new Fan($"{area.Id}.{UpperBathroom.Fan}", new UpperBathroomFanAdapter(hsrel5)));

            _sensorFactory.RegisterTemperatureSensor(area, UpperBathroom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, UpperBathroom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(area, UpperBathroom.MotionDetector, input5.GetInput(15));

            _actuatorFactory.RegisterLamp(area, UpperBathroom.LightCeilingDoor, hsrel5.GetOutput(0));
            _actuatorFactory.RegisterLamp(area, UpperBathroom.LightCeilingEdge, hsrel5.GetOutput(1));
            _actuatorFactory.RegisterLamp(area, UpperBathroom.LightCeilingMirrorCabinet, hsrel5.GetOutput(2));
            _actuatorFactory.RegisterLamp(area, UpperBathroom.LampMirrorCabinet, hsrel5.GetOutput(3));

            var combinedLights = _actuatorFactory.RegisterLogicalComponent(area, UpperBathroom.CombinedCeilingLights)
                    .WithComponent(area.GetLamp(UpperBathroom.LightCeilingDoor))
                    .WithComponent(area.GetLamp(UpperBathroom.LightCeilingEdge))
                    .WithComponent(area.GetLamp(UpperBathroom.LightCeilingMirrorCabinet))
                    .WithComponent(area.GetLamp(UpperBathroom.LampMirrorCabinet));

            _automationFactory.RegisterTurnOnAndOffAutomation(area, UpperBathroom.CombinedCeilingLightsAutomation)
                .WithTrigger(area.GetMotionDetector(UpperBathroom.MotionDetector))
                .WithTarget(combinedLights);

            new BathroomFanAutomation(
                $"{area.Id}.{UpperBathroom.FanAutomation}",
                area.GetFan(UpperBathroom.Fan),
                _schedulerService,
                _settingsService)
                .WithTrigger(area.GetMotionDetector(UpperBathroom.MotionDetector));
        }

        private class UpperBathroomFanAdapter : IFanAdapter
        {
            private readonly IBinaryOutput _relay1;
            private readonly IBinaryOutput _relay2;

            public int MaxLevel { get; } = 2;

            public UpperBathroomFanAdapter(HSREL5 hsrel5)
            {
                _relay1 = hsrel5[HSREL5Pin.Relay4];
                _relay2 = hsrel5[HSREL5Pin.GPIO0];
            }

            public void SetLevel(int level, params IHardwareParameter[] parameters)
            {
                switch (level)
                {
                    case 0:
                        {
                            _relay1.Write(BinaryState.Low);
                            _relay2.Write(BinaryState.Low);
                            break;
                        }

                    case 1:
                        {
                            _relay1.Write(BinaryState.High);
                            _relay2.Write(BinaryState.Low);
                            break;
                        }

                    case 2:
                        {
                            _relay1.Write(BinaryState.High);
                            _relay2.Write(BinaryState.High);
                            break;
                        }

                    default:
                        {
                            throw new NotSupportedException();
                        }
                }
            }
        }
    }
}
