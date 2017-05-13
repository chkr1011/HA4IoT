using System.Collections.Generic;
using System.Threading.Tasks;
using HA4IoT.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Messaging
{
    public interface IMessageBrokerService : IService
    {
        Task Publish(Message<JObject> message);

        void Subscribe(MessageSubscription subscription);

        void Unsubscribe(string subscriptionId);

        bool HasSubscribers(string topic, string payloadType);

        IList<string> GetSubscriptions();
    }
}
