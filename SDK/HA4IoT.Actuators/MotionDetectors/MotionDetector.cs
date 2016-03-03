using System;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
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

        public MotionDetector(ActuatorId id, IBinaryInput input, IHomeAutomationTimer timer, IHttpRequestController api, ILogger logger)
            : base(id, api, logger)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            
            input.StateChanged += (s, e) => HandleInputStateChanged(e);

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

        public override void HandleApiPost(ApiRequestContext context)
        {
            base.HandleApiPost(context);
            
            if (context.Request.ContainsKey("action"))
            {
                string action = context.Request.GetNamedString("action");
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

        private void HandleInputStateChanged(BinaryStateChangedEventArgs eventArgs)
        {
            // The relay at the motion detector is awlays held to high.
            // The signal is set to false if motion is detected.
            if (eventArgs.NewState == BinaryState.Low)
            {
                UpdateState(MotionDetectorState.MotionDetected);
            }
            else
            {
                UpdateState(MotionDetectorState.Idle);
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
                _motionDetectedTrigger.Invoke();
            }
            else
            {
                Logger.Verbose(Id+ ": Detection completed");
                _detectionCompletedTrigger.Invoke();
            }

            StateChanged?.Invoke(this, new MotionDetectorStateChangedEventArgs(oldState, newState));
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