using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;

namespace HA4IoT.Sensors
{
    public class SensorFactory
    {
        private readonly ITimerService _timerService;
        private readonly ISchedulerService _schedulerService;

        public SensorFactory(ITimerService timerService, ISchedulerService schedulerService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));

            _timerService = timerService;
            _schedulerService = schedulerService;
        }

        public IButton RegisterVirtualButton(IArea area, Enum id, Action<IButton> initializer = null)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var virtualButton = new Button(ComponentIdFactory.Create(area.Id, id), new EmptyButtonEndpoint(), _timerService);
            initializer?.Invoke(virtualButton);

            area.AddComponent(virtualButton);
            return virtualButton;
        }

        public IButton RegisterButton(IArea area, Enum id, IBinaryInput input, Action<IButton> initializer = null)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var button = new Button(
                ComponentIdFactory.Create(area.Id, id),
                new PortBasedButtonEndpoint(input),
                _timerService);

            initializer?.Invoke(button);

            area.AddComponent(button);
            return button;
        }

        public IArea RegisterRollerShutterButtons(
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
                ComponentIdFactory.Create(area.Id, upId),
                new PortBasedButtonEndpoint(upInput),
                _timerService);

            area.AddComponent(upButton);

            var downButton = new Button(
                ComponentIdFactory.Create(area.Id, downId),
                new PortBasedButtonEndpoint(downInput),
                _timerService);

            area.AddComponent(downButton);

            return area;
        }

        public IMotionDetector RegisterMotionDetector(IArea area, Enum id, IBinaryInput input)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var motionDetector = new MotionDetector(
                ComponentIdFactory.Create(area.Id, id),
                new PortBasedMotionDetectorEndpoint(input),
                _schedulerService);

            area.AddComponent(motionDetector);

            return motionDetector;
        }
    }
}
