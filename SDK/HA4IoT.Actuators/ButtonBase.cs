using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Actuators.Contracts;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public abstract class ButtonBase : ActuatorBase, IButton
    {
        private readonly List<Action>  _actionsForPressedShort = new List<Action>();
        private readonly List<Action> _actionsForPressedLong = new List<Action>();

        protected ButtonBase(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
        }

        public event EventHandler PressedShort;
        public event EventHandler PressedLong;

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

        protected void OnPressedShort()
        {
            NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' was pressed short.", Id);

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
            NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' was pressed long.", Id);

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
