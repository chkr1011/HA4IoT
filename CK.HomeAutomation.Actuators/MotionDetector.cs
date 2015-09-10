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
        private bool _isMotionDetected;

        public MotionDetector(string id, IBinaryInput input, HomeAutomationTimer timer, HttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            input.StateChanged += (s, e) => HandleInputStateChanged(e, notificationHandler);

            IsEnabledChanged += (s, e) =>
            {
                HandleIsEnabledStateChanged(timer, notificationHandler);
            };
        }

        public event EventHandler MotionDetected;
        public event EventHandler DetectionCompleted;

        public override void ApiGet(ApiRequestContext context)
        {
            base.ApiGet(context);
            context.Response.SetNamedValue("state", JsonValue.CreateBooleanValue(_isMotionDetected));
        }

        private void HandleInputStateChanged(BinaryStateChangedEventArgs eventArgs, INotificationHandler notificationHandler)
        {
            // The relay at the motion detector is awlays held to high.
            // The signal is set to false if motion is detected.
            if (eventArgs.State == BinaryState.Low)
            {
                _isMotionDetected = true;

                if (IsEnabled)
                {
                    notificationHandler.PublishFrom(this, NotificationType.Info, "Motion detected at '{0}'.", Id);
                    MotionDetected?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                _isMotionDetected = false;

                if (IsEnabled)
                {
                    notificationHandler.PublishFrom(this, NotificationType.Info, "Detection completed at '{0}'.", Id);
                    DetectionCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void HandleIsEnabledStateChanged(HomeAutomationTimer timer, INotificationHandler notificationHandler)
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