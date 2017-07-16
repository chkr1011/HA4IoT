using System;

namespace HA4IoT.Contracts.Messaging
{
    public class Message<TPayload> where TPayload : class
    {
        public Message(string topic, MessagePayload<TPayload> payload)
        {
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }

        public string Topic { get; }

        public MessagePayload<TPayload> Payload { get; }
    }
}
