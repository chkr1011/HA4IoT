using System;
using System.Collections.Generic;
using CK.HomeAutomation.Actuators.Contracts;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public abstract class ButtonBase : ActuatorBase, IButton
    {
        protected ButtonBase(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
        }

        protected List<Action> ShortActions { get; } = new List<Action>();
        protected List<Action> LongActions { get; } = new List<Action>();

        public event EventHandler PressedShort;
        public event EventHandler PressedLong;

        public ButtonBase WithShortAction(Action action)
        {
            ShortActions.Add(action);
            return this;
        }

        public ButtonBase WithLongAction(Action action)
        {
            LongActions.Add(action);
            return this;
        }

        protected override void ApiPost(ApiRequestContext context)
        {
            string action = context.Request.GetNamedString("duration", string.Empty);
            if (action.Equals(ButtonPressedDuration.Long.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                InvokeLongAction();
            }
            else
            {
                InvokeShortAction();
            }
        }

        protected void InvokeShortAction()
        {
            NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' was pressed short.", Id);

            PressedShort?.Invoke(this, EventArgs.Empty);
            ShortActions.ForEach(a => a.Invoke());
        }

        protected void InvokeLongAction()
        {
            NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' was pressed long.", Id);

            PressedLong?.Invoke(this, EventArgs.Empty);
            LongActions.ForEach(a => a.Invoke());
        }
    }
}
