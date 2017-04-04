using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Api
{
    public interface IApiContext
    {
        string Action { get; }
        JObject Parameter { get; }
        
        ApiResultCode ResultCode { get; set; }
        JObject Result { get; set; }
        string ResultHash { get; set; }
    }
}
