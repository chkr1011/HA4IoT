using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.Sockets
{
    public class Socket : BinaryStateActuator, ISocket
    {
        public Socket(ComponentId id, IBinaryStateEndpoint endpoint)
            : base(id, endpoint)
        {
        }
    }
}