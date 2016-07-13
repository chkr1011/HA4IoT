using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class LevelStateId
    {
        public static readonly NamedComponentState Off = BinaryStateId.Off;
        public static readonly NamedComponentState Level1 = new NamedComponentState("Level1");
        public static readonly NamedComponentState Level2 = new NamedComponentState("Level2");
        public static readonly NamedComponentState Level3 = new NamedComponentState("Level3");
    }
}
