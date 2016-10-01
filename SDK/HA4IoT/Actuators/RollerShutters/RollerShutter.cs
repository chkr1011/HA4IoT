using System;
using System.Collections.Generic;
using System.Diagnostics;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json.Linq;
using Action = HA4IoT.Actuators.Actions.Action;

namespace HA4IoT.Actuators.RollerShutters
{
    public class RollerShutter : ActuatorBase, IRollerShutter
    {
        private readonly Stopwatch _movingDuration = new Stopwatch();
        private readonly IRollerShutterEndpoint _endpoint;

        private readonly ISchedulerService _schedulerService;
        
        private readonly IAction _startMoveUpAction;
        private readonly IAction _turnOffAction;
        private readonly IAction _startMoveDownAction;

        private ComponentState _state = RollerShutterStateId.Off;

        private TimedAction _autoOffTimer;
        private int _position;

        public RollerShutter(
            ComponentId id, 
            IRollerShutterEndpoint endpoint,
            ITimerService timerService,
            ISchedulerService schedulerService,
            ISettingsService settingsService)
            : base(id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _endpoint = endpoint;
            _schedulerService = schedulerService;

            settingsService.CreateSettingsMonitor<RollerShutterSettings>(Id, s => Settings = s);

            timerService.Tick += (s, e) => UpdatePosition(e);

            _startMoveUpAction = new Action(() => SetState(RollerShutterStateId.MovingUp));
            _turnOffAction = new Action(() => SetState(RollerShutterStateId.Off));
            _startMoveDownAction = new Action(() => SetState(RollerShutterStateId.MovingDown));

            endpoint.Stop(HardwareParameter.ForceUpdateState);
        }

        public RollerShutterSettings Settings { get; private set; }

        public bool IsClosed => _position == Settings.MaxPosition;
        
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

        public override JToken ExportStatus()
        {
            var status = base.ExportStatus();
            status["Position"] = _position;
            status["IsClosed"] = IsClosed;
            return status;
        }

        public override ComponentState GetState()
        {
            return _state;
        }

        public override void ResetState()
        {
            SetState(RollerShutterStateId.Off, new ForceUpdateStateParameter());
        }

        public override void SetState(ComponentState state, params IHardwareParameter[] parameters)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (state.Equals(RollerShutterStateId.Off) || _state.Equals(state))
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
            if (apiContext.Request.Property("State") == null)
            {
                apiContext.ResultCode = ApiResultCode.InvalidBody;
                return;
            }

            var newState = new ComponentState((string) apiContext.Request["State"]);
            SetState(newState);
        }

        public override IList<ComponentState> GetSupportedStates()
        {
            return new List<ComponentState>
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
            _autoOffTimer = _schedulerService.In(Settings.AutoOffTimeout).Execute(() => SetState(RollerShutterStateId.Off));
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
            
            if (_position > Settings.MaxPosition)
            {
                _position = Settings.MaxPosition;
            }
        }
    }
}