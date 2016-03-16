using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public class Lamp : BinaryStateOutputActuator<ActuatorSettings>, ILamp
    {
        public Lamp(ActuatorId id, IBinaryStateEndpoint endpoint, IApiController apiController)
            : base(id, endpoint, apiController)
        {
            Settings = new ActuatorSettings(id);
        }
    }
}