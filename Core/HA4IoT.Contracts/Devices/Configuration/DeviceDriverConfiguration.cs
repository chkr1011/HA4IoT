using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Devices.Configuration
{
    public class DeviceDriverConfiguration
    {
        public string Type { get; set; }

        public JObject Parameters { get; set; } = new JObject();
    }
}
