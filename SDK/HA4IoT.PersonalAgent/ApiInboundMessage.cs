using System;
using HA4IoT.Contracts.PersonalAgent;

namespace HA4IoT.PersonalAgent
{
    public class ApiInboundMessage : IInboundMessage
    {
        public ApiInboundMessage(DateTime timestamp, string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            Timestamp = timestamp;
            Text = text;
        }

        public DateTime Timestamp { get; }
        public string Text { get; }
    }
}
