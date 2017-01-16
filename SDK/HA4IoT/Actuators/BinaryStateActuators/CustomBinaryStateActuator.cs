using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public class CustomBinaryStateActuator : BinaryStateActuator
    {
        public CustomBinaryStateActuator(ComponentId id, IBinaryStateAdapter endpoint)
            : base(id, endpoint)
        {
        }
    }
}