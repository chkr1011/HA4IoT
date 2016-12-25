using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Api.Cloud
{
    public class ApiResponse
    {
        public string Action { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ApiResultCode ResultCode { get; set; }

        public int InternalProcessingDuration { get; set; }

        public JObject Result { get; set; }
    }
}
