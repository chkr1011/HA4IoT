using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class ButtonStateId
    {
        public static readonly NamedComponentState Released = new NamedComponentState("Released");
        public static readonly NamedComponentState Pressed = new NamedComponentState("Pressed");
    }
}
