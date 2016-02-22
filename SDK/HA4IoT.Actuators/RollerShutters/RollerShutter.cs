using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class RollerShutter : ActuatorBase<RollerShutterSettings>, IRollerShutter
    {
        private readonly TimeSpan _autoOffTimeout;
        private readonly IBinaryOutput _directionGpioPin;
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
            IHttpRequestController httpApiController,
            ILogger logger, 
            IHomeAutomationTimer timer)
            : base(id, httpApiController, logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            _powerGpioPin = powerOutput;
            _directionGpioPin = directionOutput;
            _autoOffTimeout = autoOffTimeout;
            _timer = timer;
            
            timer.Tick += (s, e) => UpdatePosition(e);

            base.Settings = new RollerShutterSettings(id, logger);
        }

        public event EventHandler<RollerShutterStateChangedEventArgs> StateChanged; 

        public static TimeSpan DefaultMaxMovingDuration { get; } = TimeSpan.FromSeconds(20);

        public new IRollerShutterSettings Settings => base.Settings;

        public bool IsClosed => _position == Settings.MaxPosition.Value;

        public RollerShutterState GetState()
        {
            return _state;
        }

        public void SetState(RollerShutterState newState)
        {
            var oldState = _state;

            if (newState == RollerShutterState.MovingUp || newState == RollerShutterState.MovingDown)
            {
                StartMove(newState).Wait();
            }
            else
            {
                _movingDuration.Stop();

                StopInternal();

                // Ensure that the direction relay is not wasting energy.
                _directionGpioPin.Write(BinaryState.Low);

                if (oldState != RollerShutterState.Stopped)
                {
                    Logger.Info(Id + ": Stopped (Duration: " + _movingDuration.ElapsedMilliseconds + "ms)");
                }
            }

            _state = newState;
            OnStateChanged(oldState, newState);
        }

        public void TurnOff(params IParameter[] parameters)
        {
            SetState(RollerShutterState.Stopped);
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();

            status.SetNamedValue("state", _state.ToJsonValue());
            status.SetNamedValue("position", _position.ToJsonValue());
            status.SetNamedValue("positionMax", Settings.MaxPosition.Value.ToJsonValue());

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

        private async Task StartMove(RollerShutterState newState)
        {
            if (_state != RollerShutterState.Stopped)
            {
                StopInternal();

                // Ensure that the relay is completely fallen off before switching the direction.
                await Task.Delay(50);
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

            if (_position > Settings.MaxPosition.Value)
            {
                _position = Settings.MaxPosition.Value;
            }
        }
    }
}