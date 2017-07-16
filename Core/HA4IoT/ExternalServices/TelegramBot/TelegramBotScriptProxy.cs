using System;
using HA4IoT.Contracts.ExternalServices.TelegramBot;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public class TelegramBotScriptProxy : IScriptProxy
    {
        private readonly ITelegramBotService _telegramBotService;

        [MoonSharpHidden]
        public TelegramBotScriptProxy(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService ?? throw new ArgumentNullException(nameof(telegramBotService));
        }

        [MoonSharpHidden]
        public string Name => "telegramBot";

        public void SendAdminMessage(string message)
        {
            _telegramBotService.EnqueueMessageForAdministrators(message);
        }
    }
}
