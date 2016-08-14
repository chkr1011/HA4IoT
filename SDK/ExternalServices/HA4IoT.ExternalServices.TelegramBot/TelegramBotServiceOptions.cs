using System.Collections.Generic;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public class TelegramBotServiceOptions
    {
        public string AuthenticationToken { get; set; }
        public HashSet<int> Administrators { get; } = new HashSet<int>();
        public HashSet<int> ChatWhitelist { get; } = new HashSet<int>();
        public bool AllowAllClients { get; set; }
    }
}
