using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Actuators
{
    public class Socket : BinaryStateOutputActuator<ActuatorSettings>, ISocket
    {
        public Socket(ActuatorId id, IBinaryStateEndpoint endpoint, IApiController apiController)
            : base(id, endpoint, apiController)
        {
            Settings = new ActuatorSettings(id);
        }
    }
}