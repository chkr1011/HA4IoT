using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase : IActuatorBase
    {
        private bool _isEnabled = true;

        protected ActuatorBase(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            Id = id;
            NotificationHandler = notificationHandler;
            HttpApiController = httpApiController;

            ExposeToApi();
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
                IsEnabledChanged?.Invoke(this, new ActuatorIsEnabledChangedEventArgs(!value, value));
            }
        }

        protected INotificationHandler NotificationHandler { get; }

        protected IHttpRequestController HttpApiController { get; }

        public event EventHandler<ActuatorIsEnabledChangedEventArgs> IsEnabledChanged;

        public virtual void ApiPost(ApiRequestContext context)
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

        public virtual JsonObject ApiGetConfiguration()
        {
            var configuration = new JsonObject();
            configuration.SetNamedValue("id", JsonValue.CreateStringValue(Id));
            configuration.SetNamedValue("type", JsonValue.CreateStringValue(GetType().FullName));

            return configuration;
        }

        private void ExposeToApi()
        {
            HttpApiController.Handle(HttpMethod.Post, "actuator")
                .WithSegment(Id)
                .WithRequiredJsonBody()
                .Using(c =>
                {
                    var context = new ApiRequestContext(c.Request.JsonBody, new JsonObject());
                    ApiPost(context);

                    c.Response.Body = new JsonBody(context.Response);
                });

            HttpApiController.Handle(HttpMethod.Get, "actuator")
                .WithSegment(Id)
                .Using(c =>
                {
                    var context = new ApiRequestContext(c.Request.JsonBody, new JsonObject());
                    ApiGet(context);

                    c.Response.Body = new JsonBody(context.Response);
                });
        }
    }
}