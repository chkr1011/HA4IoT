using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Tests.Mockups
{
    public class TestStateMachineFactory
    {
        public TestStateMachine CreateTestStateMachineWithOnOffStates()
        {
            return CreateTestStateMachineWithOnOffStates(BinaryStateId.Off);
        }

        public TestStateMachine CreateTestStateMachineWithOnOffStates(GenericComponentState activeState)
        {
            var stateMachine = new TestStateMachine(ComponentIdGenerator.EmptyId);
            stateMachine.AddState(new StateMachineState(BinaryStateId.Off));
            stateMachine.AddState(new StateMachineState(BinaryStateId.On));
            stateMachine.ChangeState(activeState);

            return stateMachine;
        }
    }
}