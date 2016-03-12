using System;
using System.Diagnostics;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class Button : ActuatorBase<ButtonSettings>, IButton
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Trigger _pressedShortlyTrigger = new Trigger();
        private readonly Trigger _pressedLongTrigger = new Trigger();

        private ButtonState _state = ButtonState.Released;

        public Button(ActuatorId id, IButtonEndpoint endpoint, IApiController apiController, ILogger logger, IHomeAutomationTimer timer)
            : base(id, apiController, logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            Settings = new ButtonSettings(id, logger);

            timer.Tick += CheckForTimeout;
            endpoint.Pressed += (s, e) => HandleInputStateChanged(ButtonState.Pressed);
            endpoint.Released += (s, e) => HandleInputStateChanged(ButtonState.Released);
        }

        public event EventHandler<ButtonStateChangedEventArgs> StateChanged;

        public ButtonState GetState()
        {
            return _state;
        }

        public ITrigger GetPressedShortlyTrigger()
        {
            return _pressedShortlyTrigger;
        }

        public ITrigger GetPressedLongTrigger()
        {
            return _pressedLongTrigger;
        }

        protected override void HandleApiCommand(IApiContext apiContext)
        {
            string action = apiContext.Request.GetNamedString("duration", string.Empty);
            if (action.Equals(ButtonPressedDuration.Long.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                OnPressedLong();
            }
            else
            {
                OnPressedShort();
            }
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedValue("state", _state.ToJsonValue());

            return status;
        }

        private void HandleInputStateChanged(ButtonState state)
        {
            var oldState = _state;
            _state = state;

            if (!Settings.IsEnabled.Value)
            {
                return;
            }

            StateChanged?.Invoke(this, new ButtonStateChangedEventArgs(oldState, state));

            if (state == ButtonState.Pressed)
            {
                if (!_pressedLongTrigger.IsAnyAttached)
                {
                    OnPressedShort();
                }
                else
                {
                    _stopwatch.Restart();
                }
            }
            else if (state == ButtonState.Released)
            {
                if (!_stopwatch.IsRunning)
                {
                    return;
                }

                _stopwatch.Stop();
                if (_stopwatch.Elapsed >= Settings.PressedLongDuration.Value)
                {
                    OnPressedLong();
                }
                else
                {
                    OnPressedShort();
                }
            }
        }

        private void CheckForTimeout(object sender, TimerTickEventArgs e)
        {
            if (!_stopwatch.IsRunning)
            {
                return;
            }

            if (_stopwatch.Elapsed > Settings.PressedLongDuration.Value)
            {
                _stopwatch.Stop();
                OnPressedLong();
            }
        }

        private void OnPressedShort()
        {
            Logger.Info($"{Id}: pressed short");
            _pressedShortlyTrigger.Invoke();
        }

        private void OnPressedLong()
        {
            Logger.Info($"{Id}: pressed long");
            _pressedLongTrigger.Invoke();
        }
    }
}