using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Sensors.MotionDetectors
{
    public class MotionDetector : SensorBase, IMotionDetector
    {
        private readonly Trigger _motionDetectedTrigger = new Trigger();
        private readonly Trigger _detectionCompletedTrigger = new Trigger();

        private TimedAction _autoEnableAction;

        public MotionDetector(ComponentId id, IMotionDetectorEndpoint endpoint, IHomeAutomationTimer timer)
            : base(id)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            SetState(MotionDetectorStateId.Idle);

            endpoint.MotionDetected += (s, e) => UpdateState(MotionDetectorStateId.MotionDetected);
            endpoint.DetectionCompleted += (s, e) => UpdateState(MotionDetectorStateId.Idle);

            Settings.ValueChanged += (s, e) =>
            {
                if (e.SettingName == "IsEnabled")
                {
                    HandleIsEnabledStateChanged(timer);
                }
            };
        }

        public ITrigger GetMotionDetectedTrigger()
        {
            return _motionDetectedTrigger;
        }

        public ITrigger GetDetectionCompletedTrigger()
        {
            return _detectionCompletedTrigger;
        }

        public override void HandleApiCommand(IApiContext apiContext)
        {
            base.HandleApiCommand(apiContext);
            
            if (apiContext.Request.ContainsKey("action"))
            {
                string action = apiContext.Request.GetNamedString("action");
                if (action.Equals("detected", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateState(MotionDetectorStateId.MotionDetected);
                }
                else if (action.Equals("detectionCompleted", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateState(MotionDetectorStateId.Idle);
                }
            }
        }

        protected void OnMotionDetected()
        {
            Log.Info(Id + ": Motion detected");
            _motionDetectedTrigger.Execute();
        }

        protected void OnDetectionCompleted()
        {
            Log.Verbose(Id + ": Detection completed");
            _detectionCompletedTrigger.Execute();
        }

        private void UpdateState(StatefulComponentState newState)
        {
            if (!this.GetIsEnabled())
            {
                return;
            }

            SetState(newState);

            if (newState == MotionDetectorStateId.MotionDetected)
            {
                OnMotionDetected();
            }
            else
            {
                OnDetectionCompleted();
            }
        }

        private void HandleIsEnabledStateChanged(IHomeAutomationTimer timer)
        {
            if (!this.GetIsEnabled())
            {
                Log.Info(Id + ": Disabled for 1 hour");
                _autoEnableAction = timer.In(TimeSpan.FromHours(1)).Do(this.Enable);
            }
            else
            {
                _autoEnableAction?.Cancel();
            }
        }
    }
}