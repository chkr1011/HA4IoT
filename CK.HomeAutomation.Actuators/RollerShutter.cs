using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Json;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class RollerShutter : BaseActuator
    {
        private readonly Stopwatch _blindMovingDuration = new Stopwatch();
        private readonly IBinaryOutput _powerGpioPin;
        private readonly IBinaryOutput _directionGpioPin;
        private readonly TimeSpan _autoOffTimeout;
        private readonly HomeAutomationTimer _timer;
        // TODO: Add position tracker here.

        private TimedAction _autoOffTimer;

        public RollerShutter(string id, IBinaryOutput powerOutput, IBinaryOutput directionOutput, TimeSpan autoOffTimeout,
            HttpRequestController httpRequestController, INotificationHandler notificationHandler, HomeAutomationTimer timer)
            : base(id, httpRequestController, notificationHandler)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            _powerGpioPin = powerOutput;
            _directionGpioPin = directionOutput;
            _autoOffTimeout = autoOffTimeout;
            _timer = timer;

            //TODO: StartMoveUp();
        }

        public static TimeSpan DefaultMaxMovingDuration { get; } = TimeSpan.FromSeconds(20);
        public RollerShutterStatus Status { get; private set; }

        public void StartMoveUp()
        {
            StopInternal(false);
            _directionGpioPin.Write(BinaryState.Low, false);
            Start();

            Status = RollerShutterStatus.MovingUp;
            NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' started moving up.", Id);
        }

        public void StartMoveDown()
        {
            StopInternal(false);
            _directionGpioPin.Write(BinaryState.High, false);
            Start();

            Status = RollerShutterStatus.MovingDown;
            NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' started moving down.", Id);
        }

        public void Stop()
        {
            StopInternal(true);
        }
        
        public override void ApiGet(ApiRequestContext context)
        {
            context.Response.SetNamedValue("state", JsonValue.CreateStringValue(Status.ToString()));
        }

        protected override void ApiPost(ApiRequestContext context)
        {
            if (!context.Request.ContainsKey("state"))
            {
                return;
            }

            var state = (RollerShutterStatus)Enum.Parse(typeof(RollerShutterStatus), context.Request.GetNamedString("state"), true);

            if (state == RollerShutterStatus.MovingDown)
            {
                StartMoveDown();
            }
            else if (state == RollerShutterStatus.MovingUp)
            {
                StartMoveUp();
            }
            else
            {
                Stop();
            }
        }

        private void StopInternal(bool notify)
        {
            _powerGpioPin.Write(BinaryState.Low);
            _blindMovingDuration.Stop();
            
            if (notify)
            {
                Status = RollerShutterStatus.Stopped;
                NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' stopped moving.", Id);
            }

            Task.Delay(50).Wait();
            _directionGpioPin.Write(BinaryState.Low);
        }

        private void Start()
        {
            _powerGpioPin.Write(BinaryState.High);
            _blindMovingDuration.Restart();

            _autoOffTimer?.Cancel();
            _autoOffTimer = _timer.In(_autoOffTimeout).Do(Stop);
        }
    }
}