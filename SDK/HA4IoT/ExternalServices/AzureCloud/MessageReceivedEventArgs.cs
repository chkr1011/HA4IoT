using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ExternalServices.AzureCloud
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(JObject brokerProperties, JObject body)
        {
            if (brokerProperties == null) throw new ArgumentNullException(nameof(brokerProperties));
            if (body == null) throw new ArgumentNullException(nameof(body));

            BrokerProperties = brokerProperties;
            Body = body;
        }

        public JObject BrokerProperties { get; }

        public JObject Body { get; }
    }
}
