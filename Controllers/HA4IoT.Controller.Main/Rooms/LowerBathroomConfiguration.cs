using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class LowerBathroomConfiguration
    {
        private readonly IDeviceService _deviceService;
        private readonly ISchedulerService _schedulerService;
        private readonly IAreaService _areaService;
        private readonly SynonymService _synonymService;
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

            StartBathmodeButton,
            LampMirror,

            Window
        }

        public LowerBathroomConfiguration(
            IDeviceService deviceService, 
            ISchedulerService schedulerService,
            IAreaService areaService, 
            SynonymService synonymService)
        {
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));

            _deviceService = deviceService;
            _schedulerService = schedulerService;
            _areaService = areaService;
            _synonymService = synonymService;
        }

        public void Setup()
        {
            var hspe16_FloorAndLowerBathroom = _deviceService.GetDevice<HSPE16OutputOnly>(InstalledDevice.LowerFloorAndLowerBathroomHSPE16);
            var input3 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input3);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 3;

            var room = _areaService.CreateArea(Room.LowerBathroom)
                .WithMotionDetector(LowerBathroom.MotionDetector, input3.GetInput(15))
                .WithTemperatureSensor(LowerBathroom.TemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(LowerBathroom.HumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(LowerBathroom.LightCeilingDoor, hspe16_FloorAndLowerBathroom.GetOutput(0).WithInvertedState())
                .WithLamp(LowerBathroom.LightCeilingMiddle, hspe16_FloorAndLowerBathroom.GetOutput(1).WithInvertedState())
                .WithLamp(LowerBathroom.LightCeilingWindow, hspe16_FloorAndLowerBathroom.GetOutput(2).WithInvertedState())
                .WithLamp(LowerBathroom.LampMirror, hspe16_FloorAndLowerBathroom.GetOutput(4).WithInvertedState())
                .WithWindow(LowerBathroom.Window, w => w.WithCenterCasement(input3.GetInput(13), input3.GetInput(14)));

            room.WithVirtualButton(LowerBathroom.StartBathmodeButton, b => b.GetPressedShortlyTrigger().Attach(() => StartBathode(room)));

            room.CombineActuators(LowerBathroom.CombinedLights)
                .WithActuator(room.GetLamp(LowerBathroom.LightCeilingDoor))
                .WithActuator(room.GetLamp(LowerBathroom.LightCeilingMiddle))
                .WithActuator(room.GetLamp(LowerBathroom.LightCeilingWindow))
                .WithActuator(room.GetLamp(LowerBathroom.LampMirror));

            room.SetupTurnOnAndOffAutomation()
                .WithTrigger(room.GetMotionDetector(LowerBathroom.MotionDetector))
                .WithTarget(room.GetActuator(LowerBathroom.CombinedLights));

            _synonymService.AddSynonymsForArea(Room.LowerBathroom, "BadUnten", "LowerBathroom");
        }

        private void StartBathode(IArea bathroom)
        {
            bathroom.GetMotionDetector().Disable();

            bathroom.GetLamp(LowerBathroom.LightCeilingDoor).TryTurnOn();
            bathroom.GetLamp(LowerBathroom.LightCeilingMiddle).TryTurnOff();
            bathroom.GetLamp(LowerBathroom.LightCeilingWindow).TryTurnOff();
            bathroom.GetLamp(LowerBathroom.LampMirror).TryTurnOff();

            _bathmodeResetTimer?.Cancel();
            _bathmodeResetTimer = _schedulerService.In(TimeSpan.FromHours(1)).Execute(() => bathroom.GetMotionDetector().Enable());
        }
    }
}
