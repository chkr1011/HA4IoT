using HA4IoT.Contracts.PersonalAgent;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public class TelegramOutboundMessage : MessageBase, IOutboundMessage
    {
        public TelegramOutboundMessage(int chatId, string text) 
            : base(chatId, text)
        {
        }
    }
}
