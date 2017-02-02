using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class RollerShutterStateId
    {
        public static readonly GenericComponentState Off = BinaryStateId.Off;
        public static readonly GenericComponentState MovingUp = new GenericComponentState("MovingUp");
        public static readonly GenericComponentState MovingDown = new GenericComponentState("MovingDown");
    }
}
