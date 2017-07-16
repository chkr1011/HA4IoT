using System;
using System.Diagnostics;
using HA4IoT.Components;
using HA4IoT.Components.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Adapters;
using HA4IoT.Contracts.Components.Commands;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Sensors.Events;
using HA4IoT.Contracts.Settings;
using HA4IoT.Core;

namespace HA4IoT.Sensors.Buttons
{
    public class Button : ComponentBase, IButton
    {
        private readonly object _syncRoot = new object();
        private readonly IMessageBrokerService _messageBroker;
        private readonly CommandExecutor _commandExecutor = new CommandExecutor();
        private readonly Stopwatch _pressedDurationStopwatch = new Stopwatch();
        private readonly Timeout _pressedLongTimeout;
        private readonly ILogger _log;
        
        private ButtonStateValue _state = ButtonStateValue.Released;

        public Button(string id, IButtonAdapter adapter, ITimerService timerService, ISettingsService settingsService, IMessageBrokerService messageBroker, ILogService logService)
            : base(id)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));

            _log = logService?.CreatePublisher(id) ?? throw new ArgumentNullException(nameof(logService));
            settingsService.CreateSettingsMonitor<ButtonSettings>(this, s => Settings = s.NewSettings);

            _pressedLongTimeout = new Timeout(timerService);
            _pressedLongTimeout.Elapsed += (s, e) =>
            {
                _messageBroker.Publish(Id, new ButtonPressedLongEvent());
            };

            adapter.StateChanged += UpdateState;

            _commandExecutor.Register<ResetCommand>();
            _commandExecutor.Register<PressCommand>(c => PressInternal(c.Duration));
        }

        public ButtonSettings Settings { get; private set; }

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
                _messageBroker.Publish(Id, new ButtonPressedShortEvent());
            }
            else
            {
                _messageBroker.Publish(Id, new ButtonPressedLongEvent());
            }
        }

        private void UpdateState(object sender, ButtonAdapterStateChangedEventArgs e)
        {
            var state = e.State == AdapterButtonState.Pressed ? ButtonStateValue.Pressed : ButtonStateValue.Released;

            lock (_syncRoot)
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
                PublishEvents(state);
            }
        }

        private void PublishEvents(ButtonStateValue state)
        {
            if (state == ButtonStateValue.Pressed)
            {
                _pressedDurationStopwatch.Restart();

                if (!_messageBroker.HasSubscribers<ButtonPressedLongEvent>(Id))
                {
                    _messageBroker.Publish(Id, new ButtonPressedShortEvent());
                }
                else
                {
                    _pressedLongTimeout.Start(Settings.PressedLongDuration);
                }
            }
            else if (state == ButtonStateValue.Released)
            {
                _pressedDurationStopwatch.Stop();

                if (_pressedLongTimeout.IsEnabled && !_pressedLongTimeout.IsElapsed)
                {
                    _pressedLongTimeout.Stop();
                    _messageBroker.Publish(Id, new ButtonPressedShortEvent());
                }

                _log.Verbose($"Button '{Id}' pressed for {_pressedDurationStopwatch.ElapsedMilliseconds} ms.");
            }
        }
    }
}