using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public static class TelegramBotExtensions
    {
        public static bool GetIsPatternMatch(this InboundMessage inboundMessage, string pattern)
        {
            if (inboundMessage == null) throw new ArgumentNullException(nameof(inboundMessage));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            return Regex.IsMatch(inboundMessage.Text, pattern, RegexOptions.IgnoreCase);
        }

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
