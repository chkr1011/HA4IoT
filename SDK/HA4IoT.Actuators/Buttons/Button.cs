using System;
using System.Diagnostics;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public class Button : ButtonBase
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public Button(ActuatorId id, IButtonEndpoint endpoint, IApiController apiController, ILogger logger, IHomeAutomationTimer timer)
            : base(id, apiController, logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            
            timer.Tick += CheckForTimeout;
            endpoint.Pressed += (s, e) => HandleInputStateChanged(true, false);
            endpoint.Released += (s, e) => HandleInputStateChanged(false, true);
        }

        public TimeSpan TimeoutForPressedLongActions { get; set; } = TimeSpan.FromSeconds(1.5);

        private void HandleInputStateChanged(bool buttonIsPressed, bool buttonIsReleased)
        {
            if (buttonIsReleased)
            {
                SetState(ButtonState.Released);
            }
            else if (buttonIsPressed)
            {
                SetState(ButtonState.Pressed);
            }
            else
            {
                throw new NotSupportedException();
            }

            if (!Settings.IsEnabled.Value)
            {
                return;
            }

            if (buttonIsPressed)
            {
                if (!IsActionForPressedLongAttached)
                {
                    OnPressedShort();
                }
                else
                {
                    _stopwatch.Restart();
                }
            }
            else
            {
                if (!_stopwatch.IsRunning)
                {
                    return;
                }

                _stopwatch.Stop();
                if (_stopwatch.Elapsed >= TimeoutForPressedLongActions)
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

            if (_stopwatch.Elapsed > TimeoutForPressedLongActions)
            {
                _stopwatch.Stop();
                OnPressedLong();
            }
        }
    }
}