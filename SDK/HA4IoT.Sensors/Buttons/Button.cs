using System;
using System.Collections.Generic;
using System.Diagnostics;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Components;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Sensors.Buttons
{
    public class Button : SensorBase, IButton
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Trigger _pressedShortlyTrigger = new Trigger();
        private readonly Trigger _pressedLongTrigger = new Trigger();
        private readonly ButtonSettingsWrapper _settings;

        public Button(ComponentId id, IButtonEndpoint endpoint, IHomeAutomationTimer timer)
            : base(id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            _settings = new ButtonSettingsWrapper(Settings);

            SetState(ButtonStateId.Released);

            timer.Tick += CheckForTimeout;
            endpoint.Pressed += (s, e) => HandleInputStateChanged(ButtonStateId.Pressed);
            endpoint.Released += (s, e) => HandleInputStateChanged(ButtonStateId.Released);
        }
        
        public ITrigger GetPressedShortlyTrigger()
        {
            return _pressedShortlyTrigger;
        }

        public ITrigger GetPressedLongTrigger()
        {
            return _pressedLongTrigger;
        }

        public override void HandleApiCommand(IApiContext apiContext)
        {
            string action = apiContext.Request.GetNamedString("duration", string.Empty);
            if (action.Equals(ButtonPressedDuration.Long.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                OnPressedLong();
            }
            else
            {
                OnPressedShortlyShort();
            }
        }

        protected override IList<IComponentState> GetSupportedStates()
        {
            return new List<IComponentState> {ButtonStateId.Released, ButtonStateId.Pressed};
        }

        private void HandleInputStateChanged(StatefulComponentState state)
        {
            if (!this.GetIsEnabled())
            {
                return;
            }

            SetState(state);
            InvokeTriggers();
        }

        private void InvokeTriggers()
        {
            if (GetState().Equals(ButtonStateId.Pressed))
            {
                if (!_pressedLongTrigger.IsAnyAttached)
                {
                    OnPressedShortlyShort();
                }
                else
                {
                    _stopwatch.Restart();
                }
            }
            else if (GetState().Equals(ButtonStateId.Released))
            {
                if (!_stopwatch.IsRunning)
                {
                    return;
                }

                _stopwatch.Stop();
                if (_stopwatch.Elapsed >= _settings.PressedLongDuration)
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

            if (_stopwatch.Elapsed > _settings.PressedLongDuration)
            {
                _stopwatch.Stop();
                OnPressedLong();
            }
        }

        protected void OnPressedShortlyShort()
        {
            Log.Info($"{Id}: pressed short");
            _pressedShortlyTrigger.Execute();
        }

        protected void OnPressedLong()
        {
            Log.Info($"{Id}: pressed long");
            _pressedLongTrigger.Execute();
        }
    }
}