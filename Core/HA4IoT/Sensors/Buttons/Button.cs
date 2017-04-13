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
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Services.System;
using HA4IoT.Triggers;

namespace HA4IoT.Sensors.Buttons
{
    public class Button : ComponentBase, IButton
    {
        private readonly object _syncRoot = new object();

        private readonly CommandExecutor _commandExecutor = new CommandExecutor();
        private readonly ISettingsService _settingsService;
        private readonly Timeout _pressedLongTimeout;
        
        private ButtonStateValue _state = ButtonStateValue.Released;

        public Button(string id, IButtonAdapter adapter, ITimerService timerService, ISettingsService settingsService)
            : base(id)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            _pressedLongTimeout = new Timeout(timerService);
            _pressedLongTimeout.Elapsed += (s, e) => ((Trigger)PressedLongTrigger).Execute();

            adapter.Pressed += (s, e) => ProcessChangedInputState(ButtonStateValue.Pressed);
            adapter.Released += (s, e) => ProcessChangedInputState(ButtonStateValue.Released);

            _commandExecutor.Register<ResetCommand>();
            _commandExecutor.Register<PressCommand>(c => PressInternal(c.Duration));
        }

        public ButtonSettings Settings => _settingsService.GetSettings<ButtonSettings>(this);

        public ITrigger PressedShortTrigger { get; } = new Trigger();
        public ITrigger PressedLongTrigger { get; } = new Trigger();

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new ButtonState(_state));
        }

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new ButtonFeature());
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            lock (_syncRoot)
            {
                _commandExecutor.Execute(command);
            }
        }

        private void PressInternal(ButtonPressedDuration duration)
        {
            if (duration == ButtonPressedDuration.Short)
            {
                ((Trigger)PressedShortTrigger).Execute();
            }
            else
            {
                ((Trigger)PressedLongTrigger).Execute();
            }
        }
        
        private void ProcessChangedInputState(ButtonStateValue state)
        {
            if (!Settings.IsEnabled)
            {
                return;
            }

            if (_state == state)
            {
                return;
            }

            var oldState = GetState();
            _state = state;

            OnStateChanged(oldState);
            InvokeTriggers(state);
        }

        private void InvokeTriggers(ButtonStateValue state)
        {
            if (state == ButtonStateValue.Pressed)
            {
                if (!PressedLongTrigger.IsAnyAttached)
                {
                    ((Trigger)PressedShortTrigger).Execute();
                }
                else
                {
                    _pressedLongTimeout.Start(Settings.PressedLongDuration);
                }
            }
            else if (state == ButtonStateValue.Released)
            {
                if (!_pressedLongTimeout.IsElapsed)
                {
                    _pressedLongTimeout.Stop();
                    ((Trigger)PressedShortTrigger).Execute();
                }
            }
        }
    }
}