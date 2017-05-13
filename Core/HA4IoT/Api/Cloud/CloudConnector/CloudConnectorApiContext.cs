using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Api.Cloud;

namespace HA4IoT.Api.Cloud.CloudConnector
{
    public class CloudConnectorApiContext : ApiCall
    {
        public CloudConnectorApiContext(CloudRequestMessage requestMessage) 
            : base(requestMessage.Request.Action, requestMessage.Request.Parameter, requestMessage.Request.ResultHash)
        {
            if (requestMessage == null) throw new ArgumentNullException(nameof(requestMessage));

            RequestMessage = requestMessage;
        }

        public CloudRequestMessage RequestMessage { get; }
    }
}
