
namespace HA4IoT.Contracts.Actuators
{
    public static class RollerShutterStateId
    {
        public static readonly StateId Off = BinaryStateId.Off;
        public static readonly StateId MovingUp = new StateId("MovingUp");
        public static readonly StateId MovingDown = new StateId("MovingDown");
    }
}
