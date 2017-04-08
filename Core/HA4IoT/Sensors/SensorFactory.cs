using System;
using HA4IoT.Adapters;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;

namespace HA4IoT.Sensors
{
    public class SensorFactory
    {
        private readonly ITimerService _timerService;
        private readonly ISchedulerService _schedulerService;
        private readonly ISettingsService _settingsService;

        public SensorFactory(ITimerService timerService, ISchedulerService schedulerService, ISettingsService settingsService)
        {
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public ITemperatureSensor RegisterTemperatureSensor(IArea area, Enum id, INumericSensorAdapter adapter)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            var temperatureSensor = new TemperatureSensor($"{area.Id}.{id}", adapter, _settingsService);
            area.RegisterComponent(temperatureSensor);

            return temperatureSensor;
        }

        public IHumiditySensor RegisterHumiditySensor(IArea area, Enum id, INumericSensorAdapter adapter)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            var humditySensor = new HumiditySensor($"{area.Id}.{id}", adapter, _settingsService);
            area.RegisterComponent(humditySensor);

            return humditySensor;
        }

        public IButton RegisterVirtualButton(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var virtualButton = new Button($"{area.Id}.{id}", new VirtualButtonAdapter(), _timerService, _settingsService);
            area.RegisterComponent(virtualButton);

            return virtualButton;
        }

        public IButton RegisterButton(IArea area, Enum id, IBinaryInput input)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var button = new Button(
                $"{area.Id}.{id}",
                new BinaryInputButtonAdapter(input),
                _timerService,
                _settingsService);

            area.RegisterComponent(button);
            return button;
        }

        public void RegisterRollerShutterButtons(
            IArea area,
            Enum upId,
            IBinaryInput upInput,
            Enum downId,
            IBinaryInput downInput)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            var upButton = new Button(
                $"{area.Id}.{upId}",
                new BinaryInputButtonAdapter(upInput),
                _timerService,
                _settingsService);

            area.RegisterComponent(upButton);

            var downButton = new Button(
                $"{area.Id}.{downId}",
                new BinaryInputButtonAdapter(downInput),
                _timerService,
                _settingsService);

            area.RegisterComponent(downButton);
        }

        public IMotionDetector RegisterMotionDetector(IArea area, Enum id, IBinaryInput input)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var motionDetector = new MotionDetector(
                $"{area.Id}.{id}",
                new PortBasedMotionDetectorAdapter(input),
                _schedulerService,
                _settingsService);

            area.RegisterComponent(motionDetector);

            return motionDetector;
        }

        public IWindow RegisterWindow(IArea area, Enum id, IWindowAdapter adapter)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            var window = new Window($"{area.Id}.{id}", adapter, _settingsService);
            area.RegisterComponent(window);
            return window;
        }

        public IWindow RegisterWindow(IArea area, Enum id, IBinaryInput fullOpenReedSwitch, IBinaryInput tildOpenReedSwitch = null)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (fullOpenReedSwitch == null) throw new ArgumentNullException(nameof(fullOpenReedSwitch));

            var adapter = new PortBasedWindowAdapter(fullOpenReedSwitch, tildOpenReedSwitch);
            var window = new Window($"{area.Id}.{id}", adapter, _settingsService);
            area.RegisterComponent(window);
            return window;
        }
    }
}
