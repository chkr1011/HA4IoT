using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Json;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core.Timer;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class RollerShutter : ActuatorBase, IRollerShutter
    {
        private readonly TimeSpan _autoOffTimeout;
        private readonly IBinaryOutput _directionGpioPin;
        private readonly int _positionMax;
        private readonly Stopwatch _movingDuration = new Stopwatch();
        private readonly IBinaryOutput _powerGpioPin;
        private readonly IHomeAutomationTimer _timer;

        private RollerShutterState _state = RollerShutterState.Stopped;

        private TimedAction _autoOffTimer;
        private int _position;

        public RollerShutter(
            ActuatorId id, 
            IBinaryOutput powerOutput, 
            IBinaryOutput directionOutput, 
            TimeSpan autoOffTimeout,
            int maxPosition,
            IHttpRequestController api,
            ILogger logger, 
            IHomeAutomationTimer timer)
            : base(id, api, logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            _powerGpioPin = powerOutput;
            _directionGpioPin = directionOutput;
            _autoOffTimeout = autoOffTimeout;
            _positionMax = maxPosition;
            _timer = timer;

            //TODO: StartMoveUp();

            timer.Tick += (s, e) => UpdatePosition(e);
        }

        public event EventHandler<RollerShutterStateChangedEventArgs> StateChanged; 

        public static TimeSpan DefaultMaxMovingDuration { get; } = TimeSpan.FromSeconds(20);

        public bool IsClosed => _position == _positionMax;

        public RollerShutterState GetState()
        {
            return _state;
        }

        public void SetState(RollerShutterState newState)
        {
            var oldState = _state;

            if (newState == RollerShutterState.MovingUp || newState == RollerShutterState.MovingDown)
            {
                StartMove(newState);
            }
            else
            {
                _movingDuration.Stop();

                StopInternal();

                // Ensure that the direction relay is not wasting energy.
                _directionGpioPin.Write(BinaryState.Low);

                if (oldState != RollerShutterState.Stopped)
                {
                    Logger.Info(Id + ": Stopped (Duration: " +
                                             _movingDuration.ElapsedMilliseconds + "ms)");
                }
            }

            _state = newState;
            OnStateChanged(oldState, newState);
        }

        public void TurnOff(params IParameter[] parameters)
        {
            SetState(RollerShutterState.Stopped);
        }

        public override JsonObject GetStatusForApi()
        {
            var status = base.GetStatusForApi();

            status.SetNamedValue("state", JsonValue.CreateStringValue(_state.ToString()));
            status.SetNamedValue("position", JsonValue.CreateNumberValue(_position));
            status.SetNamedValue("positionMax", JsonValue.CreateNumberValue(_positionMax));

            return status;
        }

        public override void HandleApiPost(ApiRequestContext context)
        {
            if (!context.Request.ContainsKey("state"))
            {
                return;
            }

            var state = (RollerShutterState)Enum.Parse(typeof(RollerShutterState), context.Request.GetNamedString("state"), true);
            SetState(state);
        }

        private void StartMove(RollerShutterState newState)
        {
            if (_state != RollerShutterState.Stopped)
            {
                StopInternal();
                Task.Delay(50).Wait();
            }

            BinaryState binaryState;
            if (newState == RollerShutterState.MovingDown)
            {
                binaryState = BinaryState.High;
            }
            else if (newState == RollerShutterState.MovingUp)
            {
                binaryState = BinaryState.Low;
            }
            else
            {
                throw new InvalidOperationException();
            }

            _directionGpioPin.Write(binaryState, false);
            Start();
        }

        private void OnStateChanged(RollerShutterState oldState, RollerShutterState newState)
        {
            StateChanged?.Invoke(this, new RollerShutterStateChangedEventArgs(oldState, newState));
            Logger.Info(Id + ": " + oldState + "->" + newState);
        }

        private void StopInternal()
        {
            _powerGpioPin.Write(BinaryState.Low);
        }

        private void Start()
        {
            _powerGpioPin.Write(BinaryState.High);
            _movingDuration.Restart();

            _autoOffTimer?.Cancel();
            _autoOffTimer = _timer.In(_autoOffTimeout).Do(() => SetState(RollerShutterState.Stopped));
        }

        private void UpdatePosition(TimerTickEventArgs timerTickEventArgs)
        {
            if (_state == RollerShutterState.MovingUp)
            {
                _position -= (int)timerTickEventArgs.ElapsedTime.TotalMilliseconds;
            }
            else if (_state == RollerShutterState.MovingDown)
            {
                _position += (int)timerTickEventArgs.ElapsedTime.TotalMilliseconds;
            }

            if (_position < 0)
            {
                _position = 0;
            }

            if (_position > _positionMax)
            {
                _position = _positionMax;
            }
        }
    }
}