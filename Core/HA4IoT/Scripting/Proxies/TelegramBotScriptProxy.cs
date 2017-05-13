using System;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services.ExternalServices.TelegramBot;
using MoonSharp.Interpreter;

namespace HA4IoT.Scripting.Proxies
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
