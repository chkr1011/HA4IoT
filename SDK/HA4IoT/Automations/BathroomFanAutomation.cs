using System;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Automations
{
    public class BathroomFanAutomation : AutomationBase
    {
        private readonly ISchedulerService _schedulerService;
        
        private IStateMachine _actuator;
        private TimedAction _timeout;

        public BathroomFanAutomation(AutomationId id, ISchedulerService schedulerService, ISettingsService settingsService)
            : base(id)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _schedulerService = schedulerService;

            settingsService.CreateSettingsMonitor<BathroomFanAutomationSettings>(Id, s => Settings = s);
        }

        public BathroomFanAutomationSettings Settings { get; private set; }

        public BathroomFanAutomation WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.GetMotionDetectedTrigger().Triggered += TurnOn;
            motionDetector.GetDetectionCompletedTrigger().Triggered += StartTimeout;

            return this;
        }

        public BathroomFanAutomation WithActuator(IStateMachine actuator)
        {
            _actuator = actuator;
            return this;
        }
        
        private void StartTimeout(object sender, EventArgs e)
        {
            _timeout = _schedulerService.In(Settings.SlowDuration).Execute(() =>
            {
                _actuator.SetState(new ComponentState("2"));
                _timeout = _schedulerService.In(Settings.FastDuration).Execute(() => _actuator.TryTurnOff());
            });
        }

        private void TurnOn(object sender, EventArgs e)
        {
            if (!Settings.IsEnabled)
            {
                return;
            }

            _timeout?.Cancel();
            _actuator?.SetState(new ComponentState("1"));
        }
    }
}