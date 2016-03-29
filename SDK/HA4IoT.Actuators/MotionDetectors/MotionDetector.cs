using System;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class MotionDetector : ActuatorBase, IMotionDetector
    {
        private readonly Trigger _motionDetectedTrigger = new Trigger();
        private readonly Trigger _detectionCompletedTrigger = new Trigger();

        private TimedAction _autoEnableAction;
        private MotionDetectorState _state = MotionDetectorState.Idle;

        public MotionDetector(ActuatorId id, IMotionDetectorEndpoint endpoint, IHomeAutomationTimer timer)
            : base(id)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            endpoint.MotionDetected += (s, e) => UpdateState(MotionDetectorState.MotionDetected);
            endpoint.DetectionCompleted += (s, e) => UpdateState(MotionDetectorState.Idle);

            Settings.ValueChanged += (s, e) =>
            {
                if (e.SettingName == "IsEnabled")
                {
                    HandleIsEnabledStateChanged(timer);
                }
            };
        }

        public event EventHandler<MotionDetectorStateChangedEventArgs> StateChanged;

        public MotionDetectorState GetState()
        {
            return _state;
        }

        public ITrigger GetMotionDetectedTrigger()
        {
            return _motionDetectedTrigger;
        }

        public ITrigger GetDetectionCompletedTrigger()
        {
            return _detectionCompletedTrigger;
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();

            status.SetNamedValue("state", _state.ToJsonValue());

            return status;
        }

        protected override void HandleApiCommand(IApiContext apiContext)
        {
            base.HandleApiCommand(apiContext);
            
            if (apiContext.Request.ContainsKey("action"))
            {
                string action = apiContext.Request.GetNamedString("action");
                if (action.Equals("detected", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateState(MotionDetectorState.MotionDetected);
                }
                else if (action.Equals("detectionCompleted", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateState(MotionDetectorState.Idle);
                }
            }
        }

        private void UpdateState(MotionDetectorState newState)
        {
            MotionDetectorState oldState = _state;
            _state = newState;

            if (!this.GetIsEnabled())
            {
                return;
            }

            if (newState == MotionDetectorState.MotionDetected)
            {
                Log.Info(Id + ": Motion detected");
                _motionDetectedTrigger.Execute();
            }
            else
            {
                Log.Verbose(Id + ": Detection completed");
                _detectionCompletedTrigger.Execute();
            }

            StateChanged?.Invoke(this, new MotionDetectorStateChangedEventArgs(oldState, newState));
            NotifyStateChanged();
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