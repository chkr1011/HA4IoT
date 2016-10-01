using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class RollerShutterStateId
    {
        public static readonly ComponentState Off = BinaryStateId.Off;
        public static readonly ComponentState MovingUp = new ComponentState("MovingUp");
        public static readonly ComponentState MovingDown = new ComponentState("MovingDown");
    }
}
