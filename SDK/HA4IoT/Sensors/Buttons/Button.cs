using System;
using System.Collections.Generic;
using System.Diagnostics;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Triggers;

namespace HA4IoT.Sensors.Buttons
{
    public class Button : SensorBase, IButton
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public Button(ComponentId id, IButtonAdapter adapter, ITimerService timerService, ISettingsService settingsService)
            : base(id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            settingsService.CreateSettingsMonitor<ButtonSettings>(Id, s => Settings = s);

            SetState(ButtonStateId.Released);

            timerService.Tick += CheckForTimeout;

            adapter.Pressed += (s, e) => HandleInputStateChanged(ButtonStateId.Pressed);
            adapter.Released += (s, e) => HandleInputStateChanged(ButtonStateId.Released);
        }

        public IButtonSettings Settings { get; private set; }

        public ITrigger PressedShortlyTrigger { get; } = new Trigger();
        public ITrigger PressedLongTrigger { get; } = new Trigger();
    
        public override void HandleApiCall(IApiContext apiContext)
        {
            var action = (string)apiContext.Parameter["Duration"];
            if (!string.IsNullOrEmpty(action) && action.Equals(ButtonPressedDuration.Long.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                OnPressedLong();
            }
            else
            {
                OnPressedShortlyShort();
            }
        }

        public override IList<ComponentState> GetSupportedStates()
        {
            return new List<ComponentState> { ButtonStateId.Released, ButtonStateId.Pressed };
        }

        private void HandleInputStateChanged(ComponentState state)
        {
            if (!Settings.IsEnabled)
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
                if (!PressedLongTrigger.IsAnyAttached)
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