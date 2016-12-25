using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Api.Cloud;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api.Cloud.CloudConnector
{
    public class CloudConnectorApiContext : ApiContext
    {
        public CloudConnectorApiContext(CloudRequestMessage requestMessage) : base(
            requestMessage.Request.Action,
            requestMessage.Request.Parameter,
            new JObject())
        {
            if (requestMessage == null) throw new ArgumentNullException(nameof(requestMessage));

            RequestMessage = requestMessage;
        }
        
        public CloudRequestMessage RequestMessage { get; }
    }
}
