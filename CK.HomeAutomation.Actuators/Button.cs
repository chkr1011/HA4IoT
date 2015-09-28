using System;
using System.Diagnostics;
using System.Linq;
using CK.HomeAutomation.Core.Timer;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class Button : ButtonBase
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly TimeSpan _timeoutForLongAction = TimeSpan.FromSeconds(1.5);

        public Button(string id, IBinaryInput input, IHttpRequestController httpApiController, INotificationHandler notificationHandler, IHomeAutomationTimer timer)
            : base(id, httpApiController, notificationHandler)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (input == null) throw new ArgumentNullException(nameof(input));
            
            timer.Tick += CheckForTimeout;

            input.StateChanged += (s, e) =>
            {
                if (!IsEnabled)
                {
                    return;
                }

                if (e.NewState == BinaryState.High)
                {
                    if (!LongActions.Any())
                    {
                        InvokeShortAction();
                    }
                    else
                    {
                        notificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' started measuring time.", Id);
                        _stopwatch.Restart();
                    }
                }
                else
                {
                    if (!_stopwatch.IsRunning)
                    {
                        return;
                    }

                    _stopwatch.Stop();
                    if (_stopwatch.Elapsed > _timeoutForLongAction)
                    {
                        InvokeLongAction();
                    }
                    else
                    {
                        InvokeShortAction();
                    }
                }
            };
        }

        private void CheckForTimeout(object sender, TimerTickEventArgs e)
        {
            if (!_stopwatch.IsRunning)
            {
                return;
            }

            if (_stopwatch.Elapsed > _timeoutForLongAction)
            {
                _stopwatch.Stop();
                InvokeLongAction();
            }
        }
    }
}