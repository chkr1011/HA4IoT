using System;
using Windows.Data.Json;

namespace HA4IoT.Api.AzureCloud
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(JsonObject properties, JsonObject body)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if (body == null) throw new ArgumentNullException(nameof(body));

            Properties = properties;
            Body = body;
        }

        public JsonObject Properties { get; }

        public JsonObject Body { get; }
    }
}
