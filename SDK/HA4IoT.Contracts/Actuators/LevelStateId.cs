using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class LevelStateId
    {
        public static readonly ComponentState Off = BinaryStateId.Off;
        public static readonly ComponentState Level1 = new ComponentState("Level1");
        public static readonly ComponentState Level2 = new ComponentState("Level2");
        public static readonly ComponentState Level3 = new ComponentState("Level3");
    }
}
