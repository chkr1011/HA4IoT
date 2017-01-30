using Newtonsoft.Json.Linq;

namespace HA4IoT.FeatureRebuild.Api
{
    public class InvokeCommandRequest
    {
        public string ComponentId { get; set; }

        public string Command { get; set; }

        public JObject CommandParameters { get; set; }
    }
}
