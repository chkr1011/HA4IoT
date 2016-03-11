using System;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class VirtualButton : ActuatorBase<ActuatorSettings>, IButton
    {
        private readonly Trigger _pressedShortlyTrigger = new Trigger();
        private readonly Trigger _pressedLongTrigger = new Trigger();

        public VirtualButton(ActuatorId id, IApiController apiController, ILogger logger)
            : base(id, apiController, logger)
        {
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

        public override void HandleApiPost(IApiContext apiContext)
        {
            string action = apiContext.Request.GetNamedString("duration", string.Empty);
            if (action.Equals(ButtonPressedDuration.Long.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _pressedLongTrigger.Invoke();
            }
            else
            {
                _pressedShortlyTrigger.Invoke();
            }
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedValue("state", GetState().ToJsonValue());

            return status;
        }
    }
}
