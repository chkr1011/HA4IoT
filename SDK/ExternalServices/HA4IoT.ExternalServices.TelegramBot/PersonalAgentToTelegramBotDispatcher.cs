using System;
using HA4IoT.Contracts.Core;
using HA4IoT.PersonalAgent;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public class PersonalAgentToTelegramBotDispatcher
    {
        private readonly IController _controller;

        public PersonalAgentToTelegramBotDispatcher(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            _controller = controller;
        }

        public void ExposeToTelegramBot(TelegramBot telegramBot)
        {
            if (telegramBot == null) throw new ArgumentNullException(nameof(telegramBot));

            telegramBot.MessageReceived += ProcessMessageAndSendAnswer;
        }

        private async void ProcessMessageAndSendAnswer(object sender, TelegramBotMessageReceivedEventArgs e)
        {
            var messageProcessor = new PersonalAgentMessageProcessor(_controller);
            messageProcessor.ProcessMessage(e.Message);

            await e.SendResponse(messageProcessor.Answer);
        }
    }
}
