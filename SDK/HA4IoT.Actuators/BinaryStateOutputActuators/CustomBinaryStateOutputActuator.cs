using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Actuators
{
    public class CustomBinaryStateOutputActuator : BinaryStateOutputActuator<ActuatorSettings>
    {
        public CustomBinaryStateOutputActuator(ActuatorId id, IBinaryStateEndpoint endpoint, IApiController apiController)
            : base(id, endpoint, apiController)
        {
            Settings = new ActuatorSettings(id);
        }
    }
}