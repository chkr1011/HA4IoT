using System;
using HA4IoT.Contracts.Services.System;
using HA4IoT.PersonalAgent;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public class PersonalAgentToTelegramBotDispatcher
    {
        private readonly IContainerService _containerService;
        
        public PersonalAgentToTelegramBotDispatcher(IContainerService containerService)
        {
            if (containerService == null) throw new ArgumentNullException(nameof(containerService));
            
            _containerService = containerService;
        }

        public void ExposeToTelegramBot(TelegramBotService telegramBotService)
        {
            if (telegramBotService == null) throw new ArgumentNullException(nameof(telegramBotService));

            telegramBotService.MessageReceived += ProcessMessageAndSendAnswer;
        }

        private void ProcessMessageAndSendAnswer(object sender, TelegramBotMessageReceivedEventArgs e)
        {
            var messageProcessor = _containerService.GetInstance<PersonalAgentMessageProcessor>();
            messageProcessor.ProcessMessage(e.Message);

            e.EnqueueResponse(messageProcessor.Answer, TelegramMessageFormat.HTML);
        }
    }
}
