using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class BinaryStateId
    {
        public static readonly StatefulComponentState Off = new StatefulComponentState("Off");
        public static readonly StatefulComponentState On = new StatefulComponentState("On");
    }
}
