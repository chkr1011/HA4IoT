using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api.AzureCloud
{
    public class QueueBasedApiContext : ApiContext
    {
        public QueueBasedApiContext(string correlationId, string uri, JObject request, JObject response) 
            : base(uri, request, response)
        {
            if (correlationId == null) throw new ArgumentNullException(nameof(correlationId));

            CorrelationId = correlationId;
        }

        public string CorrelationId { get; }
    }
}
