using HA4IoT.Contracts.PersonalAgent;

namespace HA4IoT.Contracts.ExternalServices.TelegramBot
{
    public class TelegramOutboundMessage : MessageBase, IOutboundTextMessage
    {
        public TelegramOutboundMessage(int chatId, string text, TelegramMessageFormat format) 
            : base(chatId, text)
        {
            Format = format;
        }

        public TelegramMessageFormat Format { get; private set; }
    }
}
