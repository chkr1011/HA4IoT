using System;
using System.Diagnostics;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Api.AzureCloud
{
    public class QueueBasedApiContext : ApiContext
    {
        public QueueBasedApiContext(JsonObject brokerProperties, JsonObject message, Stopwatch processingStopwatch, ApiCallType callType, string uri, JsonObject request, JsonObject response) 
            : base(callType, uri, request, response)
        {
            if (brokerProperties == null) throw new ArgumentNullException(nameof(brokerProperties));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (processingStopwatch == null) throw new ArgumentNullException(nameof(processingStopwatch));

            BrokerProperties = brokerProperties;
            Message = message;

            ProcessingStopwatch = processingStopwatch;
        }

        public JsonObject Message { get; }

        public JsonObject BrokerProperties { get; }

        public Stopwatch ProcessingStopwatch { get; }
    }
}
