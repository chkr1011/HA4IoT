using HA4IoT.Networking.Json;

namespace HA4IoT.Settings
{
    public class ControllerSettings
    {
        [JsonMember]
        public string Language { get; set; } = "EN";

        [JsonMember]
        public string Name { get; set; } = "HA4IoT";

        [JsonMember]
        public string Description { get; set; } = "Default HA4IoT Home Automation Controller";
    }
}
