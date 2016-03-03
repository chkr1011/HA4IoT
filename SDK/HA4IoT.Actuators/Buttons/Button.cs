using System;
using System.Diagnostics;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Actuators
{
    public class Button : ButtonBase
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public Button(ActuatorId id, IBinaryInput input, IHttpRequestController api, ILogger logger, IHomeAutomationTimer timer)
            : base(id, api, logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (input == null) throw new ArgumentNullException(nameof(input));
            
            timer.Tick += CheckForTimeout;
            input.StateChanged += HandleInputStateChanged;
        }

        public TimeSpan TimeoutForPressedLongActions { get; set; } = TimeSpan.FromSeconds(1.5);

        private void HandleInputStateChanged(object sender, BinaryStateChangedEventArgs e)
        {
            bool buttonIsPressed = e.NewState == BinaryState.High;
            bool buttonIsReleased = e.NewState == BinaryState.Low;

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