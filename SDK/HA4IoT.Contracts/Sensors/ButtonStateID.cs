using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.Sensors
{
    public static class ButtonStateId
    {
        public static readonly StateId Released = new StateId("Released");
        public static readonly StateId Pressed = new StateId("Pressed");
    }
}
