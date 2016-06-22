
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class RollerShutterStateId
    {
        public static readonly NamedComponentState Off = BinaryStateId.Off;
        public static readonly NamedComponentState MovingUp = new NamedComponentState("MovingUp");
        public static readonly NamedComponentState MovingDown = new NamedComponentState("MovingDown");
    }
}
