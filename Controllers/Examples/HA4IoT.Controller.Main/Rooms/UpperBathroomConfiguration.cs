using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class UpperBathroomConfiguration
    {
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly IDeviceService _deviceService;
        private readonly ISchedulerService _schedulerService;
        private readonly IAreaRespositoryService _areaService;
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
            IDeviceService deviceService,
            ISchedulerService schedulerService,
            IAreaRespositoryService areaService,
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
            var input5 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input5);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 4;

            var area = _areaService.CreateArea(Room.UpperBathroom);

            _actuatorFactory.RegisterStateMachine(area, UpperBathroom.Fan, (s, r) => SetupFan(s, hsrel5));

            _sensorFactory.RegisterTemperatureSensor(area, UpperBathroom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, UpperBathroom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(area, UpperBathroom.MotionDetector, input5.GetInput(15));

            _actuatorFactory.RegisterLamp(area, UpperBathroom.LightCeilingDoor, hsrel5.GetOutput(0));
            _actuatorFactory.RegisterLamp(area, UpperBathroom.LightCeilingEdge, hsrel5.GetOutput(1));
            _actuatorFactory.RegisterLamp(area, UpperBathroom.LightCeilingMirrorCabinet, hsrel5.GetOutput(2));
            _actuatorFactory.RegisterLamp(area, UpperBathroom.LampMirrorCabinet, hsrel5.GetOutput(3));

            var combinedLights =
                _actuatorFactory.RegisterLogicalActuator(area, UpperBathroom.CombinedCeilingLights)
                    .WithActuator(area.GetLamp(UpperBathroom.LightCeilingDoor))
                    .WithActuator(area.GetLamp(UpperBathroom.LightCeilingEdge))
                    .WithActuator(area.GetLamp(UpperBathroom.LightCeilingMirrorCabinet))
                    .WithActuator(area.GetLamp(UpperBathroom.LampMirrorCabinet));

            _automationFactory.RegisterTurnOnAndOffAutomation(area, UpperBathroom.CombinedCeilingLightsAutomation)
                .WithTrigger(area.GetMotionDetector(UpperBathroom.MotionDetector))
                .WithTarget(combinedLights);
            
            new BathroomFanAutomation(AutomationIdGenerator.Generate(area, UpperBathroom.FanAutomation), _schedulerService, _settingsService)
                .WithTrigger(area.GetMotionDetector(UpperBathroom.MotionDetector))
                .WithActuator(area.GetStateMachine(UpperBathroom.Fan));      
        }

        private void SetupFan(StateMachine stateMachine, HSREL5 hsrel5)
        {
            var fanPort0 = hsrel5.GetOutput(4);
            var fanPort1 = hsrel5.GetOutput(5);

            stateMachine.AddOffState().WithOutput(fanPort0, BinaryState.Low).WithOutput(fanPort1, BinaryState.Low);
            stateMachine.AddState(new ComponentState("1")).WithOutput(fanPort0, BinaryState.High).WithOutput(fanPort1, BinaryState.Low);
            stateMachine.AddState(new ComponentState("2")).WithOutput(fanPort0, BinaryState.High).WithOutput(fanPort1, BinaryState.High);
            stateMachine.TryTurnOff();
        }
    }
}
