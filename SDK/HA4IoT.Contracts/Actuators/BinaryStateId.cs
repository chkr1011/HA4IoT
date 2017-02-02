using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class BinaryStateId
    {
        public static readonly GenericComponentState Off = new GenericComponentState("Off");
        public static readonly GenericComponentState On = new GenericComponentState("On");
    }
}
