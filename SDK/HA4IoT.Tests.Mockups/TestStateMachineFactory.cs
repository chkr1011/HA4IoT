using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Tests.Mockups
{
    public class TestStateMachineFactory
    {
        public TestStateMachine CreateTestStateMachine()
        {
            return new TestStateMachine(ComponentIdFactory.EmptyId);
        }

        public TestStateMachine CreateTestStateMachineWithActiveState(StateId id)
        {
            var stateMachine = new TestStateMachine(ComponentIdFactory.EmptyId);
            stateMachine.AddState(new StateMachineState(id));
            stateMachine.SetActiveState(id);

            return stateMachine;
        }

        public TestStateMachine CreateTestStateMachineWithOnOffStates()
        {
            return CreateTestStateMachineWithOnOffStates(DefaultStateId.Off);
        }

        public TestStateMachine CreateTestStateMachineWithOnOffStates(StateId activeState)
        {
            var stateMachine = new TestStateMachine(ComponentIdFactory.EmptyId);
            stateMachine.AddState(new StateMachineState(DefaultStateId.Off));
            stateMachine.AddState(new StateMachineState(DefaultStateId.On));
            stateMachine.SetActiveState(activeState);

            return stateMachine;
        }
    }
}