using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators
{
    public abstract class ButtonBase : ActuatorBase<ActuatorSettings>, IButton
    {
        private readonly Trigger _pressedShortlyTrigger = new Trigger();
        private readonly Trigger _pressedLongTrigger = new Trigger();

        private ButtonState _state = ButtonState.Released;

        protected ButtonBase(ActuatorId id, IHttpRequestController httpApiController, ILogger logger)
            : base(id, httpApiController, logger)
        {
            Settings = new ActuatorSettings(id, logger);
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

        protected void SetState(ButtonState newState)
        {
            var oldState = _state;
            _state = newState;

            if (!Settings.IsEnabled.Value)
            {
                return;
            }

            StateChanged?.Invoke(this, new ButtonStateChangedEventArgs(oldState, newState));
        }

        protected bool IsActionForPressedLongAttached => _pressedLongTrigger.IsAnyAttached;

        public override void HandleApiPost(ApiRequestContext context)
        {
            string action = context.Request.GetNamedString("duration", string.Empty);
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
            status.SetNamedValue("state", JsonValue.CreateStringValue(_state.ToString()));

            return status;
        }

        protected void OnPressedShort()
        {
            Logger.Info($"{Id}: pressed short");
            _pressedShortlyTrigger.Invoke();
        }

        protected void OnPressedLong()
        {
            Logger.Info($"{Id}: pressed long");
            _pressedLongTrigger.Invoke();
        }
    }
}
