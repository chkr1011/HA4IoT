using System;
using HA4IoT.Contracts.Api;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api.Cloud.Azure
{
    public class QueueBasedApiContext : ApiContext
    {
        public QueueBasedApiContext(string correlationId, string action, JObject parameter, string resultHash) 
            : base(action, parameter, resultHash)
        {
            if (correlationId == null) throw new ArgumentNullException(nameof(correlationId));

            CorrelationId = correlationId;
        }

        public string CorrelationId { get; }
    }
}
