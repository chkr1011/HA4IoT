namespace HA4IoT.Contracts.Actuators
{
    public static class DefaultStateId
    {
        public static readonly StateId Off = new StateId("Off");
        public static readonly StateId On = new StateId("On");

        public static readonly StateId Level1 = new StateId("Level1");
        public static readonly StateId Level2 = new StateId("Level2");
        public static readonly StateId Level3 = new StateId("Level3");

        public static readonly StateId MoveUp = new StateId("MoveUp");
        public static readonly StateId MoveDown = new StateId("MoveDown");
    }
}
