namespace HA4IoT.Contracts.Services.ExternalServices.TelegramBot
{
    public interface ITelegramBotService : IService
    {
        void EnqueueMessage(TelegramOutboundMessage message);

        void EnqueueMessageForAdministrators(string text, TelegramMessageFormat format = TelegramMessageFormat.HTML);
    }
}