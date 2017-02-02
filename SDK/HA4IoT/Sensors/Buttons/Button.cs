using System;
using System.Diagnostics;
using HA4IoT.Components;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Triggers;

namespace HA4IoT.Sensors.Buttons
{
    public class Button : ComponentBase, IButton
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private ButtonStateValue _state = ButtonStateValue.Released;

        public Button(ComponentId id, IButtonAdapter adapter, ITimerService timerService, ISettingsService settingsService)
            : base(id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            settingsService.CreateSettingsMonitor<ButtonSettings>(Id, s => Settings = s);

            timerService.Tick += CheckForTimeout;

            adapter.Pressed += (s, e) => ProcessChangedInputState(ButtonStateValue.Pressed);
            adapter.Released += (s, e) => ProcessChangedInputState(ButtonStateValue.Released);
        }

        public IButtonSettings Settings { get; private set; }

        public ITrigger PressedShortlyTrigger { get; } = new Trigger();
        public ITrigger PressedLongTrigger { get; } = new Trigger();
    
        public override ComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .WithState(new ButtonState(_state));
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection();
        }

        public override void InvokeCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var pressCommand = command as PressCommand;
            if (pressCommand == null)
            {
                throw new CommandNotSupportedException(command);
            }

            if (pressCommand.Duration == ButtonPressedDuration.Short)
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

            var oldState = new ButtonState(_state);
            _state = state;
            var newState = new ButtonState(state);

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
                    _stopwatch.Restart();
                }
            }
            else if (state == ButtonStateValue.Released)
            {
                if (!_stopwatch.IsRunning)
                {
                    return;
                }

                _stopwatch.Stop();

                if (_stopwatch.Elapsed >= Settings.PressedLongDuration)
                {
                    OnPressedLong();
                }
                else
                {
                    OnPressedShortlyShort();
                }
            }
        }

        private void CheckForTimeout(object sender, TimerTickEventArgs e)
        {
            if (!_stopwatch.IsRunning)
            {
                return;
            }

            if (_stopwatch.Elapsed > Settings.PressedLongDuration)
            {
                _stopwatch.Stop();
                OnPressedLong();
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