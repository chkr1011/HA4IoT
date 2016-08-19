using System;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Automations
{
    public class BathroomFanAutomation : AutomationBase
    {
        private readonly ISchedulerService _schedulerService;
        private IStateMachine _actuator;
        private TimeSpan _fastDuration;
        private TimeSpan _slowDuration;
        private TimedAction _timeout;

        public BathroomFanAutomation(AutomationId id, ISchedulerService schedulerService)
            : base(id)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));

            _schedulerService = schedulerService;
        }

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

        public BathroomFanAutomation WithSlowDuration(TimeSpan duration)
        {
            _slowDuration = duration;
            return this;
        }

        public BathroomFanAutomation WithFastDuration(TimeSpan duration)
        {
            _fastDuration = duration;
            return this;
        }

        private void StartTimeout(object sender, EventArgs e)
        {
            _timeout = _schedulerService.In(_slowDuration).Execute(() =>
            {
                _actuator.SetState(new NamedComponentState("2"));
                _timeout = _schedulerService.In(_fastDuration).Execute(() => _actuator.TryTurnOff());
            });
        }

        private void TurnOn(object sender, EventArgs e)
        {
            if (!this.IsEnabled())
            {
                return;
            }

            _timeout?.Cancel();
            _actuator?.SetState(new NamedComponentState("1"));
        }
    }
}