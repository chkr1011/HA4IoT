using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class LevelStateId
    {
        public static readonly StatefulComponentState Off = BinaryStateId.Off;
        public static readonly StatefulComponentState Level1 = new StatefulComponentState("Level1");
        public static readonly StatefulComponentState Level2 = new StatefulComponentState("Level2");
        public static readonly StatefulComponentState Level3 = new StatefulComponentState("Level3");
    }
}
