using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Triggers;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Messaging
{
    public static class MessageBrokerServiceExtensions
    {
        public static ITrigger CreateTrigger<TPayload>(
            this IMessageBrokerService messageBrokerService,
            string topic,
            Func<Message<TPayload>, bool> filter = null) where TPayload : class
        {
            if (messageBrokerService == null) throw new ArgumentNullException(nameof(messageBrokerService));
            if (topic == null) throw new ArgumentNullException(nameof(topic));

            var trigger = new Trigger();
            messageBrokerService.Subscribe(topic, c => trigger.Execute(), filter);

            return trigger;
        }

        public static Task Publish(this IMessageBrokerService messageBrokerService, string topic, object payload)
        {
            if (messageBrokerService == null) throw new ArgumentNullException(nameof(messageBrokerService));
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            var messagePayload = new MessagePayload<JObject>(payload.GetType().Name, JObject.FromObject(payload));
            var message = new Message<JObject>(topic, messagePayload);

            return messageBrokerService.Publish(message);
        }

        public static string Subscribe(
            this IMessageBrokerService messageBrokerService,
            string topic,
            string payloadType,
            Action<Message<JObject>> callback,
            Func<Message<JObject>, bool> filter = null)
        {
            if (messageBrokerService == null) throw new ArgumentNullException(nameof(messageBrokerService));
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var id = Guid.NewGuid().ToString();

            messageBrokerService.Subscribe(new MessageSubscription
            {
                Id = id,
                Topic = topic,
                PayloadType = payloadType,
                Filter = filter,
                Callback = callback
            });

            return id;
        }

        public static string Subscribe<TPayload>(
            this IMessageBrokerService messageBrokerService,
            string topic,
            Action<Message<TPayload>> callback,
            Func<Message<TPayload>, bool> filter = null) where TPayload : class
        {
            var id = Guid.NewGuid().ToString();

            var payloadType = typeof(TPayload).Name;
            var messageSubscription = new MessageSubscription
            {
                Id = id,
                Topic = topic,
                PayloadType = typeof(TPayload).Name,
                Callback = c => callback(new Message<TPayload>(topic, new MessagePayload<TPayload>(payloadType, c.Payload.Content.ToObject<TPayload>())))
            };

            if (filter != null)
            {
                messageSubscription.Filter = c => filter(new Message<TPayload>(topic, new MessagePayload<TPayload>(payloadType, c.Payload.Content.ToObject<TPayload>())));
            }

            messageBrokerService.Subscribe(messageSubscription);

            return id;
        }

        public static bool HasSubscribers<TPayload>(this IMessageBrokerService messageBrokerService, string topic) where TPayload : class
        {
            if (messageBrokerService == null) throw new ArgumentNullException(nameof(messageBrokerService));
            if (topic == null) throw new ArgumentNullException(nameof(topic));

            return messageBrokerService.HasSubscribers(topic, typeof(TPayload).Name);
        }
    }
}