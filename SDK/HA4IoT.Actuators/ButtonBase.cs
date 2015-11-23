using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ButtonBase : ActuatorBase, IButton
    {
        private readonly List<Action>  _actionsForPressedShort = new List<Action>();
        private readonly List<Action> _actionsForPressedLong = new List<Action>();

        private ButtonState _state = ButtonState.Released;

        protected ButtonBase(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
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

        public override void ApiPost(ApiRequestContext context)
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

        public override void ApiGet(ApiRequestContext context)
        {
            base.ApiGet(context);
           
            context.Response.SetNamedValue("state", JsonValue.CreateStringValue(_state.ToString()));
        }

        protected void OnPressedShort()
        {
            NotificationHandler.Info(Id + ": pressed short");

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
            NotificationHandler.Info(Id + ": pressed long");

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
