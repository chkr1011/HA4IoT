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
using HA4IoT.Settings;

namespace HA4IoT.Actuators.RollerShutters
{
    public class RollerShutter : ComponentBase, IRollerShutter
    {
        private readonly object _syncRoot = new object();
        private readonly IRollerShutterAdapter _adapter;
        private readonly ISettingsService _settingsService;
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
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _adapter = adapter;
            _settingsService = settingsService;

            _autoOffTimeout = new Timeout(timerService);
            _autoOffTimeout.Elapsed += (s, e) => Stop();

            timerService.Tick += (s, e) => TrackPosition(e);

            settingsService.CreateComponentSettingsMonitor<RollerShutterSettings>(Id, s => Settings = s);
        }

        public RollerShutterSettings Settings { get; set; }

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
                var commandExecutor = new CommandExecutor();
                commandExecutor.Register<MoveUpCommand>(c => MoveUp());
                commandExecutor.Register<MoveDownCommand>(c => MoveDown());
                commandExecutor.Register<TurnOffCommand>(c => Stop());
                commandExecutor.Register<ResetCommand>(c => MoveUp(true));
                commandExecutor.Execute(command);
            }
        }
        
        private void MoveUp(bool forceUpdate = false)
        {
            var oldState = GetState();

            if (forceUpdate)
            {
                _adapter.StartMoveUp(HardwareParameter.ForceUpdateState);
            }
            else
            {
                _adapter.StartMoveUp();
            }
            
            _powerState = PowerStateValue.On;
            _verticalMovingState = VerticalMovingStateValue.MovingUp;
            _autoOffTimeout.Start(Settings.AutoOffTimeout);

            OnStateChanged(oldState);
        }

        private void MoveDown()
        {
            var oldState = GetState();

            _adapter.StartMoveDown();

            _powerState = PowerStateValue.On;
            _verticalMovingState = VerticalMovingStateValue.MovingDown;
            _autoOffTimeout.Start(Settings.AutoOffTimeout);

            OnStateChanged(oldState);
        }

        private void Stop()
        {
            var oldState = GetState();

            _adapter.Stop();

            _powerState = PowerStateValue.Off;
            _verticalMovingState = VerticalMovingStateValue.Stopped;
            _autoOffTimeout.Stop();

            OnStateChanged(oldState);
        }
        
        private void TrackPosition(TimerTickEventArgs timerTickEventArgs)
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