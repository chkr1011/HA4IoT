using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators
{
    public class Lamp : BinaryStateActuator, ILamp
    {
        public Lamp(ActuatorId id, IBinaryStateEndpoint endpoint)
            : base(id, endpoint)
        {
        }
    }
}