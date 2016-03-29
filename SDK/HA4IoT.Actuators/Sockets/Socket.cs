using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators
{
    public class Socket : BinaryStateActuator, ISocket
    {
        public Socket(ActuatorId id, IBinaryStateEndpoint endpoint)
            : base(id, endpoint)
        {
        }
    }
}