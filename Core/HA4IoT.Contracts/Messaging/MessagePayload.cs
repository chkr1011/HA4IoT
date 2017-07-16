using System;

namespace HA4IoT.Contracts.Messaging
{
    public class MessagePayload<TPayload> where TPayload : class
    {
        public MessagePayload(string type, TPayload content)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public string Type { get; }

        public TPayload Content { get; }
    }
}
