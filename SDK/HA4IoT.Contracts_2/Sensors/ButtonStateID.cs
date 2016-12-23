using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class ButtonStateId
    {
        public static readonly ComponentState Released = new ComponentState("Released");
        public static readonly ComponentState Pressed = new ComponentState("Pressed");
    }
}
