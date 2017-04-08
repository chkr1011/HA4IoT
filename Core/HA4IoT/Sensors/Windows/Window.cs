using System;
using HA4IoT.Commands;
using HA4IoT.Components;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Triggers;

namespace HA4IoT.Sensors.Windows
{
    public class Window : ComponentBase, IWindow
    {
        private readonly IWindowAdapter _adapter;
        private readonly ISettingsService _settingsService;
        private WindowStateValue _state;

        public Window(string id, IWindowAdapter adapter, ISettingsService settingsService)
            : base(id)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            adapter.StateChanged += (s, e) => Update(e);
        }

        public ITrigger OpenedTrigger { get; } = new Trigger();

        public ITrigger ClosedTrigger { get; } = new Trigger();

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new WindowStateFeature());
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new WindowState(_state));
        }

        public override void ExecuteCommand(ICommand command)
        {
            var commandExecutor = new CommandExecutor();
            commandExecutor.Register<ResetCommand>(c => _adapter.Refresh());
            commandExecutor.Execute(command);
        }
        
        private void Update(WindowStateChangedEventArgs eventArgs)
        {
            WindowStateValue newState;
            if (eventArgs.OpenReedSwitchState == AdapterSwitchState.Open)
            {
                newState = WindowStateValue.Open;
            }
            else if (eventArgs.TildReedSwitchState.HasValue && eventArgs.TildReedSwitchState.Value == AdapterSwitchState.Open)
            {
                newState = WindowStateValue.TildOpen;
            }
            else
            {
                newState = WindowStateValue.Closed;
            }

            if (newState.Equals(_state))
            {
                return;
            }

            var oldState = GetState();
            _state = newState;
            
            if (!_settingsService.GetSettings<ComponentSettings>(this).IsEnabled)
            {
                return;
            }

            OnStateChanged(oldState);

            if (_state == WindowStateValue.Closed)
            {
                ((Trigger)ClosedTrigger).Execute();
            }
            else
            {
                ((Trigger)OpenedTrigger).Execute();
            }
        }
    }
}