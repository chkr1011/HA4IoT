using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Api
{
    public class ApiRequest
    {
        public string Action { get; set; }
        public JObject Parameter { get; set; }
        public string ResultHash { get; set; }
    }
}
