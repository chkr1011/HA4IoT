using System;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public class TelegramBotMessageReceivedEventArgs : EventArgs
    {
        public TelegramBotMessageReceivedEventArgs(TelegramBot telegramBot, InboundMessage message)
        {
            if (telegramBot == null) throw new ArgumentNullException(nameof(telegramBot));
            if (message == null) throw new ArgumentNullException(nameof(message));

            TelegramBot = telegramBot;
            Message = message;
        }

        public TelegramBot TelegramBot { get; }

        public InboundMessage Message { get; }
    }
}
