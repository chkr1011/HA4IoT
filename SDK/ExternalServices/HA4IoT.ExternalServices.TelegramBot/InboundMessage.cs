using System;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public class InboundMessage : MessageBase
    {
        public InboundMessage(DateTime timestamp, int chatId, string text)
            : base(chatId, text)
        {
            Timestamp = timestamp;
        }

        public DateTime Timestamp { get; }

        public OutboundMessage CreateResponse(string text)
        {
            return new OutboundMessage(ChatId, text);
        }
    }
}
