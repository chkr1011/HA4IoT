using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class ButtonStateId
    {
        public static readonly GenericComponentState Released = new GenericComponentState("Released");
        public static readonly GenericComponentState Pressed = new GenericComponentState("Pressed");
    }
}
