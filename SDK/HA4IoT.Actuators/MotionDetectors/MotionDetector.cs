using System;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class MotionDetector : ActuatorBase<ActuatorSettings>, IMotionDetector
    {
        private readonly Trigger _motionDetectedTrigger = new Trigger();
        private readonly Trigger _detectionCompletedTrigger = new Trigger();

        private TimedAction _autoEnableAction;
        private MotionDetectorState _state = MotionDetectorState.Idle;

        public MotionDetector(ActuatorId id, IMotionDetectorEndpoint endpoint, IHomeAutomationTimer timer, IApiController apiController, ILogger logger)
            : base(id, apiController, logger)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            endpoint.MotionDetected += (s, e) => UpdateState(MotionDetectorState.MotionDetected);
            endpoint.DetectionCompleted += (s, e) => UpdateState(MotionDetectorState.Idle);

            base.Settings = new ActuatorSettings(id, logger);

            Settings.IsEnabled.ValueChanged += (s, e) =>
            {
                HandleIsEnabledStateChanged(timer, logger);
            };
        }

        public event EventHandler<MotionDetectorStateChangedEventArgs> StateChanged;

        public new IActuatorSettings Settings => base.Settings;

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
            status.SetNamedValue("IsEnabled", Settings.IsEnabled.ToJsonValue());

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

            if (!Settings.IsEnabled.Value)
            {
                return;
            }

            if (newState == MotionDetectorState.MotionDetected)
            {
                Logger.Info(Id + ": Motion detected");
                _motionDetectedTrigger.Execute();
            }
            else
            {
                Logger.Verbose(Id+ ": Detection completed");
                _detectionCompletedTrigger.Execute();
            }

            StateChanged?.Invoke(this, new MotionDetectorStateChangedEventArgs(oldState, newState));
            ApiController.NotifyStateChanged(this);
        }

        private void HandleIsEnabledStateChanged(IHomeAutomationTimer timer, ILogger logger)
        {
            if (!Settings.IsEnabled.Value)
            {
                logger.Info(Id + ": Disabled for 1 hour");
                _autoEnableAction = timer.In(TimeSpan.FromHours(1)).Do(() => Settings.IsEnabled.Value = true);
            }
            else
            {
                _autoEnableAction?.Cancel();
            }
        }
    }
}