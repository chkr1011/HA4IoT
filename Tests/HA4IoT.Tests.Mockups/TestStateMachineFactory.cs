using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Tests.Mockups
{
    public class TestStateMachineFactory
    {
        public TestStateMachine CreateTestStateMachine()
        {
            return new TestStateMachine(ComponentIdGenerator.EmptyId);
        }

        public TestStateMachine CreateTestStateMachineWithActiveState(ComponentState id)
        {
            var stateMachine = new TestStateMachine(ComponentIdGenerator.EmptyId);
            stateMachine.AddState(new StateMachineState(id));
            stateMachine.SetState(id);

            return stateMachine;
        }

        public TestStateMachine CreateTestStateMachineWithOnOffStates()
        {
            return CreateTestStateMachineWithOnOffStates(BinaryStateId.Off);
        }

        public TestStateMachine CreateTestStateMachineWithOnOffStates(ComponentState activeState)
        {
            var stateMachine = new TestStateMachine(ComponentIdGenerator.EmptyId);
            stateMachine.AddState(new StateMachineState(BinaryStateId.Off));
            stateMachine.AddState(new StateMachineState(BinaryStateId.On));
            stateMachine.SetState(activeState);

            return stateMachine;
        }
    }
}