using System.Collections.Generic;
using HA4IoT.Contracts.Services.ExternalServices.TelegramBot;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.ExternalServices.TelegramBot
{
    [SettingsUri(typeof(ITelegramBotService))]
    public class TelegramBotServiceSettings
    {
        public bool IsEnabled { get; set; }
        public string AuthenticationToken { get; set; }
        public HashSet<int> Administrators { get; set; } = new HashSet<int>();
        public HashSet<int> ChatWhitelist { get; set; } = new HashSet<int>();
        public bool AllowAllClients { get; set; }
    }
}
