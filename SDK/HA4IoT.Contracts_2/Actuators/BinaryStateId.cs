using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class BinaryStateId
    {
        public static readonly ComponentState Off = new ComponentState("Off");
        public static readonly ComponentState On = new ComponentState("On");
    }
}
