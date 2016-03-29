namespace HA4IoT.Contracts.Actuators
{
    public static class DefaultStateIDs
    {
        public static readonly StateMachineStateId Off = new StateMachineStateId("Off");
        public static readonly StateMachineStateId On = new StateMachineStateId("On");

        public static readonly StateMachineStateId Level1 = new StateMachineStateId("Level1");
        public static readonly StateMachineStateId Level2 = new StateMachineStateId("Level2");
        public static readonly StateMachineStateId Level3 = new StateMachineStateId("Level3");

        public static readonly StateMachineStateId MoveUp = new StateMachineStateId("MoveUp");
        public static readonly StateMachineStateId MoveDown = new StateMachineStateId("MoveDown");
    }
}
