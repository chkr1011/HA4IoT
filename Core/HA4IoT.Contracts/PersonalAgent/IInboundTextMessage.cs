using System;

namespace HA4IoT.Contracts.PersonalAgent
{
    public interface IInboundTextMessage
    {
        DateTime Timestamp { get; }

        string Text { get; }
    }
}