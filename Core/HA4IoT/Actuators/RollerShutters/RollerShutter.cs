using System;
using HA4IoT.Commands;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Services.System;

namespace HA4IoT.Actuators.RollerShutters
{
    public class RollerShutter : ComponentBase, IRollerShutter
    {
        private readonly object _syncRoot = new object();
        private readonly CommandExecutor _commandExecutor = new CommandExecutor();
        private readonly IRollerShutterAdapter _adapter;
        private readonly Timeout _autoOffTimeout;

        private PowerStateValue _powerState = PowerStateValue.Off;
        private VerticalMovingStateValue _verticalMovingState = VerticalMovingStateValue.Stopped;
        private int _position;
        
        public RollerShutter(
            string id,
            IRollerShutterAdapter adapter,
            ITimerService timerService,
            ISettingsService settingsService)
            : base(id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

            _autoOffTimeout = new Timeout(timerService);
            _autoOffTimeout.Elapsed += (s, e) => Stop();

            timerService.Tick += TrackPosition;

            settingsService.CreateSettingsMonitor<RollerShutterSettings>(this, s => Settings = s.NewSettings);

            _commandExecutor.Register<MoveUpCommand>(c => MoveUp());
            _commandExecutor.Register<MoveDownCommand>(c => MoveDown());
            _commandExecutor.Register<TurnOffCommand>(c => Stop());
            _commandExecutor.Register<ResetCommand>(c => MoveUp(true));
        }

        public RollerShutterSettings Settings { get; private set; }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new PowerState(_powerState))
                .With(new VerticalMovingState(_verticalMovingState))
                .With(new PositionTrackingState(_position, _position == Settings.MaxPosition));
        }

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new PowerStateFeature())
                .With(new PositionTrackingFeature { MaxPosition = Settings.MaxPosition })
                .With(new VerticalMovingFeature());
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            lock (_syncRoot)
            {
                _commandExecutor.Execute(command);
            }
        }
        
        private void MoveUp(bool forceUpdate = false)
        {
            var oldState = GetState();

            if (forceUpdate)
            {
                _adapter.SetState(AdapterRollerShutterState.MoveUp, HardwareParameter.ForceUpdateState);
            }
            else
            {
                _adapter.SetState(AdapterRollerShutterState.MoveUp);
            }
            
            _powerState = PowerStateValue.On;
            _verticalMovingState = VerticalMovingStateValue.MovingUp;
            _autoOffTimeout.Start(Settings.AutoOffTimeout);

            OnStateChanged(oldState);
        }

        private void MoveDown()
        {
            var oldState = GetState();

            _adapter.SetState(AdapterRollerShutterState.MoveDown);

            _powerState = PowerStateValue.On;
            _verticalMovingState = VerticalMovingStateValue.MovingDown;
            _autoOffTimeout.Start(Settings.AutoOffTimeout);

            OnStateChanged(oldState);
        }

        private void Stop()
        {
            var oldState = GetState();

            _adapter.SetState(AdapterRollerShutterState.Stop);

            _powerState = PowerStateValue.Off;
            _verticalMovingState = VerticalMovingStateValue.Stopped;
            _autoOffTimeout.Stop();

            OnStateChanged(oldState);
        }
        
        private void TrackPosition(object sender, TimerTickEventArgs timerTickEventArgs)
        {
            if (_powerState == PowerStateValue.Off)
            {
                return;
            }

            if (_verticalMovingState == VerticalMovingStateValue.MovingUp)
            {
                _position -= (int)timerTickEventArgs.ElapsedTime.TotalMilliseconds;
            }
            else if (_verticalMovingState == VerticalMovingStateValue.MovingDown)
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