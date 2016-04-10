using System;
using System.Diagnostics;
using Windows.Data.Json;
using HA4IoT.Actuators.Actions;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Networking;
using Action = HA4IoT.Actuators.Actions.Action;

namespace HA4IoT.Actuators.RollerShutters
{
    public class RollerShutter : StateMachine, IRollerShutter
    {
        private readonly Stopwatch _movingDuration = new Stopwatch();
        private readonly IHomeAutomationTimer _timer;

        private readonly IAction _startMoveUpAction;
        private readonly IAction _turnOffAction;
        private readonly IAction _startMoveDownAction;

        private readonly RollerShutterSettingsWrapper _settings;
        
        private TimedAction _autoOffTimer;
        private int _position;

        public RollerShutter(
            ComponentId id, 
            IRollerShutterEndpoint endpoint,
            IHomeAutomationTimer timer)
            : base(id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            _timer = timer;
            timer.Tick += (s, e) => UpdatePosition(e);
            _settings = new RollerShutterSettingsWrapper(Settings);

            _startMoveUpAction = new Action(() => SetState(RollerShutterStateId.MovingUp));
            _turnOffAction = new Action(() => SetState(RollerShutterStateId.Off));
            _startMoveDownAction = new Action(() => SetState(RollerShutterStateId.MovingDown));

            endpoint.Stop(HardwareParameter.ForceUpdateState);

            AddState(new StateMachineState(RollerShutterStateId.Off).WithAction(endpoint.Stop));
            AddState(new StateMachineState(RollerShutterStateId.MovingUp).WithAction(endpoint.StartMoveUp));
            AddState(new StateMachineState(RollerShutterStateId.MovingDown).WithAction(endpoint.StartMoveDown));

            SetInitialState(RollerShutterStateId.Off);
        }

        public bool IsClosed => _position == _settings.MaxPosition;
        
        public IAction GetTurnOffAction()
        {
            return _turnOffAction;
        }

        public IAction GetStartMoveUpAction()
        {
            return _startMoveUpAction;
        }

        public IAction GetStartMoveDownAction()
        {
            return _startMoveDownAction;
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();

            status.SetNamedNumber("position", _position);
            
            return status;
        }

        protected override void OnActiveStateChanged(IStateMachineState oldState, IStateMachineState newState)
        {
            base.OnActiveStateChanged(oldState, newState);

            if (newState.Id == RollerShutterStateId.MovingUp)
            {
                RestartTracking();
            }
            else if (newState.Id == RollerShutterStateId.MovingDown)
            {
                RestartTracking();
            }
        }

        public override void HandleApiCommand(IApiContext apiContext)
        {
            var state = new StatefulComponentState(apiContext.Request.GetNamedString("state"));
            SetState(state);
        }

        private void RestartTracking()
        {
            _movingDuration.Restart();

            _autoOffTimer?.Cancel();
            _autoOffTimer = _timer.In(_settings.AutoOffTimeout).Do(() => SetState(RollerShutterStateId.Off));
        }

        private void UpdatePosition(TimerTickEventArgs timerTickEventArgs)
        {
            var activeState = GetState();

            if (activeState == RollerShutterStateId.MovingUp)
            {
                _position -= (int)timerTickEventArgs.ElapsedTime.TotalMilliseconds;
            }
            else if (activeState == RollerShutterStateId.MovingDown)
            {
                _position += (int)timerTickEventArgs.ElapsedTime.TotalMilliseconds;
            }

            if (_position < 0)
            {
                _position = 0;
            }

            if (_position > _settings.MaxPosition)
            {
                _position = _settings.MaxPosition;
            }
        }
    }
}