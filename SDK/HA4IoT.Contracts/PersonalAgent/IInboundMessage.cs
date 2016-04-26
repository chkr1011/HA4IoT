using System;

namespace HA4IoT.Contracts.PersonalAgent
{
    public interface IInboundMessage
    {
        DateTime Timestamp { get; }

        string Text { get; }
    }
}