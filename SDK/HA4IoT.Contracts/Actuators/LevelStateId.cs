namespace HA4IoT.Contracts.Actuators
{
    public static class LevelStateId
    {
        public static readonly StateId Off = BinaryStateId.Off;
        public static readonly StateId Level1 = new StateId("Level1");
        public static readonly StateId Level2 = new StateId("Level2");
        public static readonly StateId Level3 = new StateId("Level3");
    }
}
