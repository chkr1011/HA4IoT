
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class RollerShutterStateId
    {
        public static readonly StatefulComponentState Off = BinaryStateId.Off;
        public static readonly StatefulComponentState MovingUp = new StatefulComponentState("MovingUp");
        public static readonly StatefulComponentState MovingDown = new StatefulComponentState("MovingDown");
    }
}
