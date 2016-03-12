using Windows.Data.Json;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Api.AzureCloud
{
    public class QueueBasedApiContext : ApiContext
    {
        public QueueBasedApiContext(string correlationId, ApiCallType callType, string uri, JsonObject request, JsonObject response) 
            : base(callType, uri, request, response)
        {
            CorrelationId = correlationId;
        }

        public string CorrelationId { get; }
    }
}
