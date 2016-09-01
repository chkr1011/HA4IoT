using System;
using HA4IoT.Contracts.PersonalAgent;

namespace HA4IoT.Contracts.Services.ExternalServices.TelegramBot
{
    public class TelegramInboundMessage : MessageBase, IInboundMessage
    {
        public TelegramInboundMessage(DateTime timestamp, int chatId, string text)
            : base(chatId, text)
        {
            Timestamp = timestamp;
        }

        public DateTime Timestamp { get; }

        public TelegramOutboundMessage CreateResponse(string text, TelegramMessageFormat format = TelegramMessageFormat.HTML)
        {
            return new TelegramOutboundMessage(ChatId, text, format);
        }
    }
}
