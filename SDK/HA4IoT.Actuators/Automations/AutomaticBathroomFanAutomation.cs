using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Core.Timer;

namespace HA4IoT.Actuators.Automations
{
    public class AutomaticBathroomFanAutomation
    {
        private readonly IHomeAutomationTimer _timer;
        private StateMachine _actuator;
        private TimeSpan _fastDuration;
        private TimeSpan _slowDuration;
        private TimedAction _timeout;

        public AutomaticBathroomFanAutomation(IHomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public AutomaticBathroomFanAutomation WithMotiWithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.MotionDetected += TurnOn;
            motionDetector.DetectionCompleted += StartTimeout;

            return this;
        }

        public AutomaticBathroomFanAutomation WithActuator(StateMachine actuator)
        {
            _actuator = actuator;
            return this;
        }

        public AutomaticBathroomFanAutomation WithSlowDuration(TimeSpan duration)
        {
            _slowDuration = duration;
            return this;
        }

        public AutomaticBathroomFanAutomation WithFastDuration(TimeSpan duration)
        {
            _fastDuration = duration;
            return this;
        }

        private void StartTimeout(object sender, EventArgs e)
        {
            _timeout = _timer.In(_slowDuration).Do(() =>
            {
                _actuator.SetState("2");
                _timeout = _timer.In(_fastDuration).Do(() => _actuator.TurnOff());
            });
        }

        private void TurnOn(object sender, EventArgs e)
        {
            _timeout?.Cancel();
            _actuator?.SetState("1");
        }
    }
}