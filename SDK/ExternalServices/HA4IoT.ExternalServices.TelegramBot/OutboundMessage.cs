namespace HA4IoT.ExternalServices.TelegramBot
{
    public class OutboundMessage : MessageBase
    {
        public OutboundMessage(int chatId, string text) 
            : base(chatId, text)
        {
        }
    }
}
