using System;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class VirtualButton : ActuatorBase<ActuatorSettings>, IButton
    {
        private readonly Trigger _pressedShortlyTrigger = new Trigger();
        private readonly Trigger _pressedLongTrigger = new Trigger();

        public VirtualButton(ActuatorId id, IApiController apiController)
            : base(id, apiController)
        {
            Settings = new ActuatorSettings(id);
        }

        public event EventHandler<ButtonStateChangedEventArgs> StateChanged;

        public ButtonState GetState()
        {
            return ButtonState.Released;
        }

        public ITrigger GetPressedShortlyTrigger()
        {
            return _pressedShortlyTrigger;
        }

        public ITrigger GetPressedLongTrigger()
        {
            return _pressedLongTrigger;
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedValue("state", GetState().ToJsonValue());

            return status;
        }

        protected override void HandleApiCommand(IApiContext apiContext)
        {
            string action = apiContext.Request.GetNamedString("duration", string.Empty);
            if (action.Equals(ButtonPressedDuration.Long.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _pressedLongTrigger.Execute();
            }
            else
            {
                _pressedShortlyTrigger.Execute();
            }

            OnStateChanged(ButtonState.Released, ButtonState.Pressed);
            OnStateChanged(ButtonState.Pressed, ButtonState.Released);
        }

        public void ForwardApiCommand(IApiContext apiContext)
        {
            if (apiContext == null) throw new ArgumentNullException(nameof(apiContext));

            HandleApiCommand(apiContext);
        }

        private void OnStateChanged(ButtonState oldState, ButtonState newState)
        {
            StateChanged?.Invoke(this, new ButtonStateChangedEventArgs(oldState, newState));
            ApiController.NotifyStateChanged(this);
        }
    }
}
