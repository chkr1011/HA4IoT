using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Logging;
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
        private readonly Timeout _pressedLongTimeout;

        private ButtonStateValue _state = ButtonStateValue.Released;

        public Button(string id, IButtonAdapter adapter, ITimerService timerService, ISettingsService settingsService)
            : base(id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            settingsService.CreateSettingsMonitor<ButtonSettings>(Id, s => Settings = s);

            _pressedLongTimeout = new Timeout(timerService);
            _pressedLongTimeout.Elapsed += (s, e) => OnPressedLong();

            adapter.Pressed += (s, e) => ProcessChangedInputState(ButtonStateValue.Pressed);
            adapter.Released += (s, e) => ProcessChangedInputState(ButtonStateValue.Released);
        }

        public IButtonSettings Settings { get; private set; }

        public ITrigger PressedShortlyTrigger { get; } = new Trigger();
        public ITrigger PressedLongTrigger { get; } = new Trigger();

        public override ComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new ButtonState(_state));
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new ButtonFeature());
        }

        public override void InvokeCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandInvoker = new CommandInvoker();
            commandInvoker.Register<ResetCommand>();
            commandInvoker.Register<PressCommand>(c => PressInternal(c.Duration));
            commandInvoker.Invoke(command);
        }

        private void PressInternal(ButtonPressedDuration duration)
        {
            if (duration == ButtonPressedDuration.Short)
            {
                OnPressedShortlyShort();
            }
            else
            {
                OnPressedLong();
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
            var newState = GetState();

            OnStateChanged(oldState, newState);
            InvokeTriggers(state);
        }

        private void InvokeTriggers(ButtonStateValue state)
        {
            if (state == ButtonStateValue.Pressed)
            {
                if (!PressedLongTrigger.IsAnyAttached)
                {
                    OnPressedShortlyShort();
                }
                else
                {
                    _pressedLongTimeout.Start(Settings.PressedLongDuration);
                }
            }
            else if (state == ButtonStateValue.Released)
            {
                if (_pressedLongTimeout.IsRunning)
                {
                    _pressedLongTimeout.Stop();
                    OnPressedShortlyShort();
                }
            }
        }

        protected void OnPressedShortlyShort()
        {
            Log.Info($"{Id}: pressed short");
            ((Trigger)PressedShortlyTrigger).Execute();
        }

        protected void OnPressedLong()
        {
            Log.Info($"{Id}: pressed long");
            ((Trigger)PressedLongTrigger).Execute();
        }
    }
}