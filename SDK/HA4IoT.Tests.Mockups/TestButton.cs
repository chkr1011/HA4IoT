using System;
using Windows.Data.Json;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Tests.Mockups
{
    public class TestButton : IButton
    {
        private readonly Trigger _pressedLongTrigger = new Trigger();
        private readonly Trigger _pressedShortlyTrigger = new Trigger();

        public TestButton()
        {
            Settings = new ActuatorSettings(ActuatorIdFactory.EmptyId, new TestLogger());
        }

        public event EventHandler<ButtonStateChangedEventArgs> StateChanged;
        public ActuatorId Id { get; }
        public IActuatorSettings Settings { get; }
        public ButtonState State { get; } = ButtonState.Released;

        public JsonObject ExportConfigurationToJsonObject()
        {
            return new JsonObject();
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

        public JsonObject ExportStatusToJsonObject()
        {
            return new JsonObject();
        }

        public void LoadSettings()
        {
        }

        public void ExposeToApi()
        {
            
        }

        public void PressShort()
        {
            _pressedShortlyTrigger.Invoke();
        }

        public void PressLong()
        {
            _pressedLongTrigger.Invoke();
        }
    }
}