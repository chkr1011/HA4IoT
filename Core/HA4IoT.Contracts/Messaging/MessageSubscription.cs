using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Messaging
{
    public class MessageSubscription
    {
        public string Id { get; set; }

        public string Topic { get; set; }

        public string PayloadType { get; set; }

        public Func<Message<JObject>, bool> Filter { get; set; }

        public Action<Message<JObject>> Callback { get; set; }
    }
}
