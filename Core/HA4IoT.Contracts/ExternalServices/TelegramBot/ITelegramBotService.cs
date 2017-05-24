using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.ExternalServices.TelegramBot
{
    public interface ITelegramBotService : IService
    {
        void EnqueueMessage(TelegramOutboundMessage message);

        void EnqueueMessageForAdministrators(string text, TelegramMessageFormat format = TelegramMessageFormat.HTML);
    }
}