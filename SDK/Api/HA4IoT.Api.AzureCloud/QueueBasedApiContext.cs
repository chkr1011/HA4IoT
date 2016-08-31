using System;
using System.Diagnostics;
using HA4IoT.Contracts.Api;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api.AzureCloud
{
    public class QueueBasedApiContext : ApiContext
    {
        public QueueBasedApiContext(JObject brokerProperties, JObject message, Stopwatch processingStopwatch, ApiCallType callType, string uri, JObject request, JObject response) 
            : base(callType, uri, request, response)
        {
            if (brokerProperties == null) throw new ArgumentNullException(nameof(brokerProperties));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (processingStopwatch == null) throw new ArgumentNullException(nameof(processingStopwatch));

            BrokerProperties = brokerProperties;
            Message = message;

            ProcessingStopwatch = processingStopwatch;
        }

        public JObject Message { get; }

        public JObject BrokerProperties { get; }

        public Stopwatch ProcessingStopwatch { get; }
    }
}
