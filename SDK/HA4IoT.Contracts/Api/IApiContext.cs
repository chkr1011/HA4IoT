using Windows.Data.Json;

namespace HA4IoT.Contracts.Api
{
    public interface IApiContext
    {
        ApiCallType CallType { get; }
        string Uri { get; }
        JsonObject Request { get; }
        ApiResultCode ResultCode { get; set; }
        JsonObject Response { get; set; }
    }
}
