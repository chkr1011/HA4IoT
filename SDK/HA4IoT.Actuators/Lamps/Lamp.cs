using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class Lamp : BinaryStateOutputActuator
    {
        public Lamp(ActuatorId id, IBinaryOutput output, IHttpRequestController request, ILogger logger)
            : base(id, output, request, logger)
        {
        }
    }
}