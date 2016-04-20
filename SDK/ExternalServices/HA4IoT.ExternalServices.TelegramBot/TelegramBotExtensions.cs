using System;
using System.Threading.Tasks;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public static class TelegramBotExtensions
    {
        public static async Task<bool> SendResponse(this TelegramBotMessageReceivedEventArgs messageReceivedEventArgs, string text)
        {
            if (messageReceivedEventArgs == null) throw new ArgumentNullException(nameof(messageReceivedEventArgs));
            if (text == null) throw new ArgumentNullException(nameof(text));

            return await
                messageReceivedEventArgs.TelegramBot.TrySendMessageAsync(
                    messageReceivedEventArgs.Message.CreateResponse(text));
        }
    }
}
