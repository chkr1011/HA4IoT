using System;
using HA4IoT.Contracts.PersonalAgent;

namespace HA4IoT.Tests.Mockups
{
    public class TestInboundMessage : IInboundTextMessage
    {
        public TestInboundMessage(string text)
        {
            Text = text;
        }

        public DateTime Timestamp { get; set; }
        public string Text { get; }
    }
}
