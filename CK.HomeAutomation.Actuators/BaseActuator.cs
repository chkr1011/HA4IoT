using System;
using Windows.Data.Json;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public abstract class BaseActuator
    {
        private bool _isEnabled = true;

        protected BaseActuator(string id, HttpRequestController httpApiController, INotificationHandler notificationHandler)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            Id = id;
            NotificationHandler = notificationHandler;
            
            ExposeToApi(httpApiController);
        }

        public string Id { get; }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }

            set
            {
                if (_isEnabled == value)
                {
                    return;
                }

                _isEnabled = value;
                IsEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected INotificationHandler NotificationHandler { get; }

        public event EventHandler IsEnabledChanged;

        protected virtual void ApiPost(ApiRequestContext context)
        {
            if (context.Request.ContainsKey("isEnabled"))
            {
                IsEnabled = context.Request.GetNamedBoolean("isEnabled", false);
                NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' {1}.", Id, IsEnabled ? "enabled" : "disabled");
            }
        }

        public virtual void ApiGet(ApiRequestContext context)
        {
            context.Response.SetNamedValue("isEnabled", JsonValue.CreateBooleanValue(IsEnabled));
        }

        private void ExposeToApi(HttpRequestController httpApiController)
        {
            httpApiController.Handle(HttpMethod.Post, "actuator")
                .WithSegment(Id)
                .WithRequiredJsonBody()
                .Using(c =>
                {
                    var context = new ApiRequestContext(c.Request.JsonBody, new JsonObject());
                    ApiPost(context);

                    c.Response.Result = context.Response;
                });

            httpApiController.Handle(HttpMethod.Get, "actuator")
                .WithSegment(Id)
                .Using(c =>
                {
                    var context = new ApiRequestContext(c.Request.JsonBody, new JsonObject());
                    ApiGet(context);

                    c.Response.Result = context.Response;
                });
        }
    }
}