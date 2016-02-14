using System;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Tests.Mockups
{
    public class TestButton : IButton
    {
        private readonly Trigger _pressedShortlyTrigger = new Trigger();
        private readonly Trigger _pressedLongTrigger = new Trigger();

        public event EventHandler<ActuatorIsEnabledChangedEventArgs> IsEnabledChanged;
        public event EventHandler<ButtonStateChangedEventArgs> StateChanged;

        public ActuatorId Id { get; }
        public bool IsEnabled { get; }

        public ButtonState State { get; set; } = ButtonState.Released;

        public JsonObject GetConfigurationForApi()
        {
            return new JsonObject();
        }

        public void PressShort()
        {
            _pressedShortlyTrigger.Invoke();
        }

        public void PressLong()
        {
            _pressedLongTrigger.Invoke();
        }

        public ButtonState GetState()
        {
            return State;
        }

        public ITrigger GetPressedShortlyTrigger()
        {
            return _pressedShortlyTrigger;
        }

        public ITrigger GetPressedLongTrigger()
        {
            return _pressedLongTrigger;
        }

        public JsonObject GetStatusForApi()
        {
            return new JsonObject();
        }
    }
}
