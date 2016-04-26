using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.Animations
{
    public class AnimateParameter : IHardwareParameter
    {
        public bool Reverse { get; set; }

        public AnimateParameter WithReversedOrder()
        {
            Reverse = true;
            return this;
        }
    }
}
