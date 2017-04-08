using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Api
{
    public class ApiResponse
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ApiResultCode ResultCode { get; set; }
        public JObject Result { get; set; }
        public string ResultHash { get; set; }
    }
}
