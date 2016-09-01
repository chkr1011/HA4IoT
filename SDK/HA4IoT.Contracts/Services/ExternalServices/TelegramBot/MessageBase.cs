using System;

namespace HA4IoT.Contracts.Services.ExternalServices.TelegramBot
{
    public abstract class MessageBase
    {
        public MessageBase(int chatId, string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            ChatId = chatId;
            Text = text;
        }

        public int ChatId { get; }

        public string Text { get; }
    }
}
