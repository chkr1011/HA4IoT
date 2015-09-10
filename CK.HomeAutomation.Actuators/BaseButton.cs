using System;
using System.Collections.Generic;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public abstract class BaseButton : BaseActuator, IButton
    {
        protected BaseButton(string id, HttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
            Handler = notificationHandler;
        }

        protected List<Action> ShortActions { get; } = new List<Action>();
        protected List<Action> LongActions { get; } = new List<Action>();

        protected INotificationHandler Handler { get; }

        public event EventHandler PressedShort;
        public event EventHandler PressedLong;

        public BaseButton WithShortAction(Action action)
        {
            ShortActions.Add(action);
            return this;
        }

        public BaseButton WithLongAction(Action action)
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
            Handler.PublishFrom(this, NotificationType.Info, "'{0}' was pressed short.", Id);

            ShortActions.ForEach(a => a.Invoke());
            PressedShort?.Invoke(this, EventArgs.Empty);
        }

        protected void InvokeLongAction()
        {
            Handler.PublishFrom(this, NotificationType.Info, "'{0}' was pressed long.", Id);

            LongActions.ForEach(a => a.Invoke());
            PressedLong?.Invoke(this, EventArgs.Empty);
        }
    }
}
