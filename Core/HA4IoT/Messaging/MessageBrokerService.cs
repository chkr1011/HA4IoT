using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using HA4IoT.Core;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Messaging
{
    public class MessageBrokerService : ServiceBase, IMessageBrokerService
    {
        public const string AnyWildcard = "*";

        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, MessageSubscription> _subscriptions = new Dictionary<string, MessageSubscription>();
        private readonly ILogger _log;

        public MessageBrokerService(ILogService logService, IScriptingService scriptingService)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            _log = logService?.CreatePublisher(nameof(MessageBrokerService)) ?? throw new ArgumentNullException(nameof(logService));

            scriptingService.RegisterScriptProxy(s => new MessageBrokerScriptProxy(this, s));
        }

        public Task Publish(Message<JObject> message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (Controller.IsRunningInUnitTest)
            {
                PublishInternal(message);
                return Task.FromResult((object)null);
            }

            return Task.Run(() => PublishInternal(message));
        }

        public void Subscribe(MessageSubscription subscription)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            if (subscription.Id == null) throw new ArgumentNullException(nameof(subscription.Id));
            if (subscription.Topic == null) throw new ArgumentNullException(nameof(subscription.Topic));
            if (subscription.Callback == null) throw new ArgumentNullException(nameof(subscription.Callback));

            lock (_syncRoot)
            {
                if (_subscriptions.ContainsKey(subscription.Id))
                {
                    throw new Exception($"Subscription '{subscription.Id}' already registered.");
                }

                _subscriptions.Add(subscription.Id, subscription);
            }

            _log.Info($"Created subscription '{subscription.Id}' for topic '{subscription.Topic}' and type '{subscription.PayloadType}'.");
        }

        public void Unsubscribe(string subscriptionId)
        {
            if (subscriptionId == null) throw new ArgumentNullException(nameof(subscriptionId));

            lock (_syncRoot)
            {
                _subscriptions.Remove(subscriptionId);
            }

            _log.Info($"Removed subscription '{subscriptionId}'.");
        }

        public bool HasSubscribers(string topic, string payloadType)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (payloadType == null) throw new ArgumentNullException(nameof(payloadType));

            lock (_syncRoot)
            {
                return _subscriptions.Values.Any(s => string.Equals(s.Topic, topic) && string.Equals(s.PayloadType, payloadType));
            }
        }

        public IList<string> GetSubscriptions()
        {
            lock (_syncRoot)
            {
                return _subscriptions.Keys.ToList();
            }
        }

        private void PublishInternal(Message<JObject> message)
        {
            _log.Verbose($"Message for topic '{message.Topic}' with payload of type '{message.Payload.Type}' published.");

            List<MessageSubscription> subscriptions;
            lock (_syncRoot)
            {
                subscriptions = new List<MessageSubscription>(_subscriptions.Values);
            }

            var affectedSubscriptions = subscriptions.Where(s => IsMatch(message, s)).ToList();

            var tasks = new Task[affectedSubscriptions.Count];
            for (var i = 0; i < affectedSubscriptions.Count; i++)
            {
                var subscription = affectedSubscriptions[i];
                tasks[i] = Task.Run(() => ExecuteCallback(subscription, message));
            }

            Task.WaitAll(tasks);
        }

        private void ExecuteCallback(MessageSubscription messageSubscription, Message<JObject> message)
        {
            try
            {
                messageSubscription.Callback(message);
            }
            catch (Exception exception)
            {
                _log.Warning(exception, $"Error while executing callback of subscription '{messageSubscription.Id}'.");
            }
        }

        private static bool IsMatch(Message<JObject> message, MessageSubscription subscription)
        {
            if (subscription.Topic != AnyWildcard && !string.Equals(subscription.Topic, message.Topic))
            {
                return false;
            }

            if (!string.Equals(subscription.PayloadType, message.Payload.Type))
            {
                return false;
            }

            if (subscription.Filter != null && !subscription.Filter(message))
            {
                return false;
            }

            return true;
        }
    }
}