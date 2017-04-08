using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Automations
{
    public class BathroomFanAutomation : AutomationBase
    {
        private readonly ISchedulerService _schedulerService;
        
        private readonly IFan _fan;
        private IDelayedAction _delayedAction;

        public BathroomFanAutomation(string id, IFan fan, ISchedulerService schedulerService, ISettingsService settingsService)
            : base(id)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _fan = fan ?? throw new ArgumentNullException(nameof(fan));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

            settingsService.CreateSettingsMonitor<BathroomFanAutomationSettings>(this, s => Settings = s.NewSettings);
        }

        public BathroomFanAutomationSettings Settings { get; private set; }

        public BathroomFanAutomation WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.MotionDetectedTrigger.Triggered += (_, __) => TurnOnSlow();
            motionDetector.MotionDetectionCompletedTrigger.Triggered += (_, __) => StartTimeout();

            return this;
        }

        private void StartTimeout()
        {
            _delayedAction?.Cancel();

            _delayedAction = _schedulerService.In(Settings.SlowDuration, () =>
            {
                if (_fan.GetFeatures().Extract<LevelStateFeature>().MaxLevel > 1)
                {
                    _fan.TrySetLevel(2);
                    _delayedAction = _schedulerService.In(Settings.FastDuration, () => _fan.TryTurnOff());
                }
                else
                {
                    _fan.TryTurnOff();
                }
            });
        }

        private void TurnOnSlow()
        {
            if (!Settings.IsEnabled)
            {
                return;
            }

            _delayedAction?.Cancel();
            _fan.TrySetLevel(1);
        }
    }
}