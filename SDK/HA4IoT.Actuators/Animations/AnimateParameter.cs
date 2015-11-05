using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators.Animations
{
    public class AnimateParameter : IParameter
    {
        public bool Reverse { get; set; }

        public AnimateParameter WithReversedOrder()
        {
            Reverse = true;
            return this;
        }
    }
}
