using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Automations
{
    public class BathroomFanAutomation : AutomationBase
    {
        private readonly ISchedulerService _schedulerService;
        
        private readonly IFan _fan;
        private TimedAction _timeout;

        public BathroomFanAutomation(string id, IFan fan, ISchedulerService schedulerService, ISettingsService settingsService)
            : base(id)
        {
            if (fan == null) throw new ArgumentNullException(nameof(fan));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _fan = fan;
            _schedulerService = schedulerService;

            settingsService.CreateSettingsMonitor<BathroomFanAutomationSettings>(Id, s => Settings = s);
        }

        public BathroomFanAutomationSettings Settings { get; private set; }

        public BathroomFanAutomation WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.MotionDetectedTrigger.Triggered += TurnOn;
            motionDetector.MotionDetectionCompletedTrigger.Triggered += StartTimeout;

            return this;
        }

        private void StartTimeout(object sender, EventArgs e)
        {
            _timeout = _schedulerService.In(Settings.SlowDuration).Execute(() =>
            {
                _fan.TrySetLevel(2);
                _timeout = _schedulerService.In(Settings.FastDuration).Execute(() => _fan.TryTurnOff());
            });
        }

        private void TurnOn(object sender, EventArgs e)
        {
            if (!Settings.IsEnabled)
            {
                return;
            }

            _timeout?.Cancel();
            _fan.TrySetLevel(1);
        }
    }
}