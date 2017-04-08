using System.Collections.Generic;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public class TelegramBotServiceSettings
    {
        public bool IsEnabled { get; set; }
        public string AuthenticationToken { get; set; }
        public HashSet<int> Administrators { get; set; } = new HashSet<int>();
        public HashSet<int> ChatWhitelist { get; set; } = new HashSet<int>();
        public bool AllowAllClients { get; set; }
    }
}
