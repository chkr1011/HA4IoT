using System;
using Windows.Data.Json;

namespace HA4IoT.Api.AzureCloud
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(JsonObject brokerProperties, JsonObject body)
        {
            if (brokerProperties == null) throw new ArgumentNullException(nameof(brokerProperties));
            if (body == null) throw new ArgumentNullException(nameof(body));

            BrokerProperties = brokerProperties;
            Body = body;
        }

        public JsonObject BrokerProperties { get; }

        public JsonObject Body { get; }
    }
}
