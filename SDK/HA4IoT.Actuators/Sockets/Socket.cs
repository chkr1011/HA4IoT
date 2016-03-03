using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Actuators
{
    public class Socket : BinaryStateOutputActuator<ActuatorSettings>
    {
        public Socket(ActuatorId id, IBinaryOutput output, IHttpRequestController httpApiController, ILogger logger)
            : base(id, output, httpApiController, logger)
        {
            Settings = new ActuatorSettings(id, logger);
        }
    }
}