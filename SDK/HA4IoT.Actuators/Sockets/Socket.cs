using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Actuators
{
    public class Socket : BinaryStateOutputActuator<ActuatorSettings>, ISocket
    {
        public Socket(ActuatorId id, IBinaryStateEndpoint endpoint, IHttpRequestController httpApiController, ILogger logger)
            : base(id, endpoint, httpApiController, logger)
        {
            Settings = new ActuatorSettings(id, logger);
        }
    }
}