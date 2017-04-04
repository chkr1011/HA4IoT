using Newtonsoft.Json.Linq;

namespace HA4IoT.Settings
{
    public class SettingsServiceApiRequest
    {
        public string Uri { get; set; }

        public JObject Settings { get; set; }
    }
}
