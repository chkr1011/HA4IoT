using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Json;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core.Timer;
using HA4IoT.Hardware;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public class RollerShutter : ActuatorBase
    {
        private readonly TimeSpan _autoOffTimeout;
        private readonly IBinaryOutput _directionGpioPin;
        private readonly int _maxPosition;
        private readonly Stopwatch _movingDuration = new Stopwatch();
        private readonly IBinaryOutput _powerGpioPin;
        private readonly IHomeAutomationTimer _timer;

        private TimedAction _autoOffTimer;
        private int _position;

        public RollerShutter(
            string id, 
            IBinaryOutput powerOutput, 
            IBinaryOutput directionOutput, 
            TimeSpan autoOffTimeout,
            int maxPosition,
            IHttpRequestController httpApiController,
            INotificationHandler notificationHandler, 
            IHomeAutomationTimer timer)
            : base(id, httpApiController, notificationHandler)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            _powerGpioPin = powerOutput;
            _directionGpioPin = directionOutput;
            _autoOffTimeout = autoOffTimeout;
            _maxPosition = maxPosition;
            _timer = timer;

            //TODO: StartMoveUp();

            timer.Tick += (s, e) => UpdatePosition(e);
        }

        public static TimeSpan DefaultMaxMovingDuration { get; } = TimeSpan.FromSeconds(20);
        public RollerShutterState State { get; private set; }
        public bool IsClosed => _position == _maxPosition;

        public void StartMoveUp()
        {
            _movingDuration.Restart();

            StopInternal();
            _directionGpioPin.Write(BinaryState.Low, false);
            Start();

            State = RollerShutterState.MovingUp;
            NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' started moving up.", Id);
        }

        public void StartMoveDown()
        {
            _movingDuration.Restart();

            StopInternal();
            _directionGpioPin.Write(BinaryState.High, false);
            Start();

            State = RollerShutterState.MovingDown;
            NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' started moving down.", Id);
        }

        public void Stop()
        {
            _movingDuration.Stop();

            StopInternal();
            _directionGpioPin.Write(BinaryState.Low);

            if (State != RollerShutterState.Stopped)
            {
                State = RollerShutterState.Stopped;
                NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' stopped moving (Duration: {1}ms).", Id, _movingDuration.ElapsedMilliseconds);
            }
        }

        public override void ApiGet(ApiRequestContext context)
        {
            context.Response.SetNamedValue("state", JsonValue.CreateStringValue(State.ToString()));
            context.Response.SetNamedValue("position", JsonValue.CreateNumberValue(_position));
        }

        public override void ApiPost(ApiRequestContext context)
        {
            if (!context.Request.ContainsKey("state"))
            {
                return;
            }

            var state = (RollerShutterState)Enum.Parse(typeof(RollerShutterState), context.Request.GetNamedString("state"), true);

            if (state == RollerShutterState.MovingDown)
            {
                StartMoveDown();
            }
            else if (state == RollerShutterState.MovingUp)
            {
                StartMoveUp();
            }
            else
            {
                Stop();
            }
        }

        private void StopInternal()
        {
            _powerGpioPin.Write(BinaryState.Low);
            Task.Delay(50).Wait();
        }

        private void Start()
        {
            _powerGpioPin.Write(BinaryState.High);
            _movingDuration.Restart();

            _autoOffTimer?.Cancel();
            _autoOffTimer = _timer.In(_autoOffTimeout).Do(Stop);
        }

        private void UpdatePosition(TimerTickEventArgs timerTickEventArgs)
        {
            if (State == RollerShutterState.MovingUp)
            {
                _position -= (int)timerTickEventArgs.ElapsedTime.TotalMilliseconds;
            }
            else if (State == RollerShutterState.MovingDown)
            {
                _position += (int)timerTickEventArgs.ElapsedTime.TotalMilliseconds;
            }

            if (_position < 0)
            {
                _position = 0;
            }

            if (_position > _maxPosition)
            {
                _position = _maxPosition;
            }
        }
    }
}