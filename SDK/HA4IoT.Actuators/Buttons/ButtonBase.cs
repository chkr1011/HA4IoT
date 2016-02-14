using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ButtonBase : ActuatorBase, IButton
    {
        private readonly List<Action>  _actionsForPressedShort = new List<Action>();
        private readonly List<Action> _actionsForPressedLong = new List<Action>();

        private ButtonState _state = ButtonState.Released;

        protected ButtonBase(ActuatorId id, IHttpRequestController api, ILogger logger)
            : base(id, api, logger)
        {
        }

        public event EventHandler PressedShort;
        public event EventHandler PressedLong;
        public event EventHandler<ButtonStateChangedEventArgs> StateChanged;

        public ButtonState GetState()
        {
            return _state;
        }

        protected void SetState(ButtonState newState)
        {
            var oldState = _state;
            _state = newState;

            if (!IsEnabled)
            {
                return;
            }

            StateChanged?.Invoke(this, new ButtonStateChangedEventArgs(oldState, newState));
        }

        protected bool IsActionForPressedLongAttached => _actionsForPressedLong.Any() || PressedLong != null;

        public ButtonBase WithShortAction(Action action)
        {
            _actionsForPressedShort.Add(action);
            return this;
        }

        public ButtonBase WithLongAction(Action action)
        {
            _actionsForPressedLong.Add(action);
            return this;
        }

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

        public override JsonObject GetStatusForApi()
        {
            var status = base.GetStatusForApi();
            status.SetNamedValue("state", JsonValue.CreateStringValue(_state.ToString()));

            return status;
        }

        protected void OnPressedShort()
        {
            Logger.Info(Id + ": pressed short");

            try
            {
                PressedShort?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                _actionsForPressedShort.ForEach(a => a.Invoke());
            }
        }

        protected void OnPressedLong()
        {
            Logger.Info(Id + ": pressed long");

            try
            {
                PressedLong?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                _actionsForPressedLong.ForEach(a => a.Invoke());
            }
        }
    }
}
