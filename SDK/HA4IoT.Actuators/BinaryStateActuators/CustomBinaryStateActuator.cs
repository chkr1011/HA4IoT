using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators
{
    public class CustomBinaryStateActuator : BinaryStateActuator
    {
        public CustomBinaryStateActuator(ActuatorId id, IBinaryStateEndpoint endpoint)
            : base(id, endpoint)
        {
        }
    }
}