using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class MotionDetector : ActuatorBase, IMotionDetector
    {
        private TimedAction _autoEnableAction;
        private MotionDetectorState _state = MotionDetectorState.Idle;

        public MotionDetector(ActuatorId id, IBinaryInput input, IHomeAutomationTimer timer, IHttpRequestController api, ILogger logger)
            : base(id, api, logger)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            
            input.StateChanged += (s, e) => HandleInputStateChanged(e);

            IsEnabledChanged += (s, e) =>
            {
                HandleIsEnabledStateChanged(timer, logger);
            };
        }

        public event EventHandler MotionDetected;
        public event EventHandler DetectionCompleted;
        public event EventHandler<MotionDetectorStateChangedEventArgs> StateChanged;

        public MotionDetectorState GetState()
        {
            return _state;
        }

        public override JsonObject GetStatusForApi()
        {
            var status = base.GetStatusForApi();
            status.SetNamedValue("state", JsonValue.CreateStringValue(_state.ToString()));

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

            if (!IsEnabled)
            {
                return;
            }

            if (newState == MotionDetectorState.MotionDetected)
            {
                Logger.Info(Id + ": Motion detected");
                MotionDetected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Logger.Verbose(Id+ ": Detection completed");
                DetectionCompleted?.Invoke(this, EventArgs.Empty);
            }

            StateChanged?.Invoke(this, new MotionDetectorStateChangedEventArgs(oldState, newState));
        }

        private void HandleIsEnabledStateChanged(IHomeAutomationTimer timer, ILogger logger)
        {
            if (!IsEnabled)
            {
                logger.Info(Id + ": Disabled for 1 hour");
                _autoEnableAction = timer.In(TimeSpan.FromHours(1)).Do(() => IsEnabled = true);
            }
            else
            {
                _autoEnableAction?.Cancel();
            }
        }
    }
}