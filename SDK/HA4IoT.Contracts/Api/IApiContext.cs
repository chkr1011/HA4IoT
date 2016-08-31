using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Api
{
    public interface IApiContext
    {
        ApiCallType CallType { get; }
        string Uri { get; }
        JObject Request { get; }
        ApiResultCode ResultCode { get; set; }
        JObject Response { get; set; }
    }
}
