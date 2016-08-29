using HA4IoT.Contracts.PersonalAgent;

namespace HA4IoT.Contracts.Services.ExternalServices.TelegramBot
{
    public class TelegramOutboundMessage : MessageBase, IOutboundMessage
    {
        public TelegramOutboundMessage(int chatId, string text, TelegramMessageFormat format) 
            : base(chatId, text)
        {
            Format = format;
        }

        public TelegramMessageFormat Format { get; private set; }
    }
}
