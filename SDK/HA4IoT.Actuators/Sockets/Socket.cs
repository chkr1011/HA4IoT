using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class Socket : BinaryStateOutputActuator
    {
        public Socket(ActuatorId id, IBinaryOutput output, IHttpRequestController httpApiController, ILogger logger)
            : base(id, output, httpApiController, logger)
        {
        }
    }
}