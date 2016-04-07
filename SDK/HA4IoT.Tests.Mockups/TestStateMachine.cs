using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Tests.Mockups
{
    public class TestStateMachine : StateMachine
    {
        public TestStateMachine(ComponentId id) 
            : base(id)
        {
        }
    }
}
