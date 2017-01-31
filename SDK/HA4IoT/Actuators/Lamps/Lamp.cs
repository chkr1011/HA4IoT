using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.Lamps
{
    public class Lamp : BinaryStateActuator, ILamp
    {
        public Lamp(ComponentId id, IBinaryOutputComponentAdapter endpoint)
            : base(id, endpoint)
        {
        }
    }
}