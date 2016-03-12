using System;
using System.Diagnostics;
using Windows.Data.Json;
using HA4IoT.Actuators.Actions;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class RollerShutter : ActuatorBase<RollerShutterSettings>, IRollerShutter
    {
        private readonly object _syncRoot = new object();
        private readonly Stopwatch _movingDuration = new Stopwatch();
        private readonly IHomeAutomationTimer _timer;
        private readonly IRollerShutterEndpoint _endpoint;

        private readonly IHomeAutomationAction _startMoveUpAction;
        private readonly IHomeAutomationAction _turnOffAction;
        private readonly IHomeAutomationAction _startMoveDownAction;

        private RollerShutterState _state = RollerShutterState.Stopped;

        private TimedAction _autoOffTimer;
        private int _position;

        public RollerShutter(
            ActuatorId id, 
            IRollerShutterEndpoint endpoint,
            IApiController apiController,
            ILogger logger, 
            IHomeAutomationTimer timer)
            : base(id, apiController, logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            _endpoint = endpoint;
            _timer = timer;
            timer.Tick += (s, e) => UpdatePosition(e);

            base.Settings = new RollerShutterSettings(id, logger);

            _startMoveUpAction = new HomeAutomationAction(() => SetState(RollerShutterState.MovingUp));
            _turnOffAction = new HomeAutomationAction(() => SetState(RollerShutterState.Stopped));
            _startMoveDownAction = new HomeAutomationAction(() => SetState(RollerShutterState.MovingDown));

            _endpoint.Stop();
        }

        public event EventHandler<RollerShutterStateChangedEventArgs> StateChanged; 

        public new IRollerShutterSettings Settings => base.Settings;

        public bool IsClosed => _position == Settings.MaxPosition.Value;

        public RollerShutterState GetState()
        {
            lock (_syncRoot)
            {
                return _state;
            }
        }

        public void SetState(RollerShutterState state)
        {
            RollerShutterState oldState;
            lock (_syncRoot)
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

                oldState = _state;
                _state = state;
                
                Logger.Info($"{Id}:{oldState}->{state}");
            }

            StateChanged?.Invoke(this, new RollerShutterStateChangedEventArgs(oldState, _state));
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            SetState(RollerShutterState.Stopped);
        }

        public IHomeAutomationAction GetTurnOffAction()
        {
            return _turnOffAction;
        }

        public IHomeAutomationAction GetStartMoveUpAction()
        {
            return _startMoveUpAction;
        }

        public IHomeAutomationAction GetStartMoveDownAction()
        {
            return _startMoveDownAction;
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();

            status.SetNamedValue("state", _state.ToJsonValue());
            status.SetNamedValue("position", _position.ToJsonValue());
            status.SetNamedValue("positionMax", Settings.MaxPosition.Value.ToJsonValue());

            return status;
        }

        protected override void HandleApiCommand(IApiContext apiContext)
        {
            if (!apiContext.Request.ContainsKey("state"))
            {
                return;
            }

            var state = (RollerShutterState)Enum.Parse(typeof(RollerShutterState), apiContext.Request.GetNamedString("state"), true);
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