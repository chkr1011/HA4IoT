using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Actuators
{
    public class CustomBinaryStateOutputActuator : BinaryStateOutputActuator<ActuatorSettings>
    {
        public CustomBinaryStateOutputActuator(ActuatorId id, IBinaryStateEndpoint endpoint, IHttpRequestController request, ILogger logger)
            : base(id, endpoint, request, logger)
        {
            Settings = new ActuatorSettings(id, logger);
        }
    }
}