using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class ButtonStateId
    {
        public static readonly StatefulComponentState Released = new StatefulComponentState("Released");
        public static readonly StatefulComponentState Pressed = new StatefulComponentState("Pressed");
    }
}
