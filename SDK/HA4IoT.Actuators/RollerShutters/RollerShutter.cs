using System;
using System.Diagnostics;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class RollerShutter : ActuatorBase<RollerShutterSettings>, IRollerShutter
    {
        private readonly Stopwatch _movingDuration = new Stopwatch();
        private readonly IHomeAutomationTimer _timer;
        private readonly IRollerShutterEndpoint _endpoint;

        private RollerShutterState _state = RollerShutterState.Stopped;

        private TimedAction _autoOffTimer;
        private int _position;

        public RollerShutter(
            ActuatorId id, 
            IRollerShutterEndpoint endpoint,
            IHttpRequestController httpApiController,
            ILogger logger, 
            IHomeAutomationTimer timer)
            : base(id, httpApiController, logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            _endpoint = endpoint;
            _timer = timer;
            timer.Tick += (s, e) => UpdatePosition(e);

            base.Settings = new RollerShutterSettings(id, logger);

            _endpoint.Stop();
        }

        public event EventHandler<RollerShutterStateChangedEventArgs> StateChanged; 

        public new IRollerShutterSettings Settings => base.Settings;

        public bool IsClosed => _position == Settings.MaxPosition.Value;

        public RollerShutterState GetState()
        {
            return _state;
        }

        public void SetState(RollerShutterState state)
        {
            if (state == _state)
            {
                return;
            }

            if (state == RollerShutterState.Stopped)
            {
                _endpoint.Stop();
                Logger.Info($"{Id}: Stopped (Duration: {_movingDuration.ElapsedMilliseconds} ms)");
            }
            else if (state == RollerShutterState.MovingUp)
            {
                _endpoint.StartMoveUp();
                RestartTracking();
            }
            else if (state == RollerShutterState.MovingDown)
            {
                _endpoint.StartMoveDown();
                RestartTracking();
            }

            RollerShutterState oldState = _state;
            _state = state;

            StateChanged?.Invoke(this, new RollerShutterStateChangedEventArgs(oldState, _state));
            Logger.Info($"{Id}:{oldState}->{state}");
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

        private void RestartTracking()
        {
            _movingDuration.Restart();

            _autoOffTimer?.Cancel();
            _autoOffTimer = _timer.In(Settings.AutoOffTimeout.Value).Do(() => SetState(RollerShutterState.Stopped));
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