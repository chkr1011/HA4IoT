using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Hardware.RemoteSockets.Configuration
{
    public class RemoteSocketCodeGeneratorConfiguration
    {
        public string Type { get; set; }

        public JObject Parameters { get; set; } = new JObject();
    }
}
