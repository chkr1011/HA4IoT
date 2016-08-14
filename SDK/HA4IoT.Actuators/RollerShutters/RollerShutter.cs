using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Data.Json;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking;
using Action = HA4IoT.Actuators.Actions.Action;

namespace HA4IoT.Actuators.RollerShutters
{
    public class RollerShutter : ActuatorBase, IRollerShutter
    {
        private readonly Stopwatch _movingDuration = new Stopwatch();
        private readonly IRollerShutterEndpoint _endpoint;
        private readonly ITimerService _timerService;
        private readonly ISchedulerService _schedulerService;

        private readonly IAction _startMoveUpAction;
        private readonly IAction _turnOffAction;
        private readonly IAction _startMoveDownAction;

        private readonly RollerShutterSettingsWrapper _settings;

        private IComponentState _state = RollerShutterStateId.Off;

        private TimedAction _autoOffTimer;
        private int _position;

        public RollerShutter(
            ComponentId id, 
            IRollerShutterEndpoint endpoint,
            ITimerService timerService,
            ISchedulerService schedulerService)
            : base(id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));

            _endpoint = endpoint;
            _timerService = timerService;
            _schedulerService = schedulerService;

            timerService.Tick += (s, e) => UpdatePosition(e);
            _settings = new RollerShutterSettingsWrapper(Settings);

            _startMoveUpAction = new Action(() => SetState(RollerShutterStateId.MovingUp));
            _turnOffAction = new Action(() => SetState(RollerShutterStateId.Off));
            _startMoveDownAction = new Action(() => SetState(RollerShutterStateId.MovingDown));

            endpoint.Stop(HardwareParameter.ForceUpdateState);
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

        public override IComponentState GetState()
        {
            return _state;
        }

        public override void ResetState()
        {
            SetState(RollerShutterStateId.Off, new ForceUpdateStateParameter());
        }

        public override void SetState(IComponentState state, params IHardwareParameter[] parameters)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (state.Equals(_state))
            {
                return;
            }

            if (state.Equals(RollerShutterStateId.Off))
            {
                _endpoint.Stop(parameters);
            }
            else if (state.Equals(RollerShutterStateId.MovingUp))
            {
                _endpoint.StartMoveUp(parameters);
                RestartTracking();
            }
            else if (state.Equals(RollerShutterStateId.MovingDown))
            {
                _endpoint.StartMoveDown(parameters);
                RestartTracking();
            }

            var oldState = _state;
            _state = state;

            OnActiveStateChanged(oldState, _state);
        }

        public override void HandleApiCall(IApiContext apiContext)
        {
            if (apiContext.CallType == ApiCallType.Command)
            {
                var state = new NamedComponentState(apiContext.Request.GetNamedString("state"));
                SetState(state);
            }
        }

        public override IList<IComponentState> GetSupportedStates()
        {
            return new List<IComponentState>
            {
                RollerShutterStateId.Off,
                RollerShutterStateId.MovingUp,
                RollerShutterStateId.MovingDown
            };
        }

        private void RestartTracking()
        {
            _movingDuration.Restart();

            _autoOffTimer?.Cancel();
            _autoOffTimer = _schedulerService.In(_settings.AutoOffTimeout).Execute(() => SetState(RollerShutterStateId.Off));
        }

        private void UpdatePosition(TimerTickEventArgs timerTickEventArgs)
        {
            var activeState = GetState();

            if (activeState.Equals(RollerShutterStateId.MovingUp))
            {
                _position -= (int)timerTickEventArgs.ElapsedTime.TotalMilliseconds;
            }
            else if (activeState.Equals(RollerShutterStateId.MovingDown))
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