using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class LevelStateId
    {
        public static readonly GenericComponentState Off = BinaryStateId.Off;
        public static readonly GenericComponentState Level1 = new GenericComponentState("Level1");
        public static readonly GenericComponentState Level2 = new GenericComponentState("Level2");
        public static readonly GenericComponentState Level3 = new GenericComponentState("Level3");
    }
}
