using System;
using Windows.Data.Json;
using CK.HomeAutomation.Core.Timer;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class MotionDetector : BaseActuator
    {
        private TimedAction _autoEnableAction;

        public MotionDetector(string id, IBinaryInput input, IHomeAutomationTimer timer, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            
            input.StateChanged += (s, e) => HandleInputStateChanged(e);

            IsEnabledChanged += (s, e) =>
            {
                HandleIsEnabledStateChanged(timer, notificationHandler);
            };
        }

        public event EventHandler MotionDetected;
        public event EventHandler DetectionCompleted;
        public bool IsMotionDetected { get; private set; }

        public override void ApiGet(ApiRequestContext context)
        {
            base.ApiGet(context);
            context.Response.SetNamedValue("state", JsonValue.CreateBooleanValue(IsMotionDetected));
        }

        protected override void ApiPost(ApiRequestContext context)
        {
            base.ApiPost(context);
            string action = context.Request.GetNamedString("action");

            if (action.Equals("detected", StringComparison.OrdinalIgnoreCase))
            {
                OnMotionDetected();
            }
            else if (action.Equals("detectionCompleted", StringComparison.OrdinalIgnoreCase))
            {
                OnDetectionCompleted();
            }
        }

        private void HandleInputStateChanged(BinaryStateChangedEventArgs eventArgs)
        {
            // The relay at the motion detector is awlays held to high.
            // The signal is set to false if motion is detected.
            if (eventArgs.NewState == BinaryState.Low)
            {
                OnMotionDetected();
            }
            else
            {
                OnDetectionCompleted();
            }
        }

        private void OnMotionDetected()
        {
            IsMotionDetected = true;

            if (IsEnabled)
            {
                NotificationHandler.PublishFrom(this, NotificationType.Info, "Motion detected at '{0}'.", Id);
                MotionDetected?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnDetectionCompleted()
        {
            IsMotionDetected = false;

            if (IsEnabled)
            {
                NotificationHandler.PublishFrom(this, NotificationType.Info, "Detection completed at '{0}'.", Id);
                DetectionCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private void HandleIsEnabledStateChanged(IHomeAutomationTimer timer, INotificationHandler notificationHandler)
        {
            if (!IsEnabled)
            {
                notificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' disabled for 1 hour.");
                _autoEnableAction = timer.In(TimeSpan.FromHours(1)).Do(() => IsEnabled = true);
            }
            else
            {
                _autoEnableAction?.Cancel();
            }
        }
    }
}