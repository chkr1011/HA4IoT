using FluentAssertions;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Components.Tests
{
    [TestClass]
    public class ComponentComparisonStateTests
    {
        [TestMethod]
        public void NumericState_Equals_NumericState()
        {
            IComponentState state1 = new NumericSensorValue(5);
            IComponentState state2 = new NumericSensorValue(5);

            state1.Equals(state2).ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void NumericState_NotEquals_NumericState()
        {
            IComponentState state1 = new NumericSensorValue(5);
            IComponentState state2 = new NumericSensorValue(6);

            state1.Equals(state2).ShouldBeEquivalentTo(false);
        }

        [TestMethod]
        public void State_Equals_State()
        {
            IComponentState state1 = new StatefulComponentState("Off");
            IComponentState state2 = new StatefulComponentState("Off");

            state1.Equals(state2).ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void State_NotEquals_State()
        {
            IComponentState state1 = new StatefulComponentState("Off");
            IComponentState state2 = new StatefulComponentState("On");

            state1.Equals(state2).ShouldBeEquivalentTo(false);
        }

        [TestMethod]
        public void State_NotEquals_DifferentState()
        {
            IComponentState state1 = new StatefulComponentState("Off");
            IComponentState state2 = new NumericSensorValue(5);

            state1.Equals(state2).ShouldBeEquivalentTo(false);
        }

        [TestMethod]
        public void UnknownState_NotEquals_DifferentState()
        {
            IComponentState state1 = new UnknownComponentState();
            IComponentState state2 = new NumericSensorValue(5);

            state1.Equals(state2).ShouldBeEquivalentTo(false);
        }

        [TestMethod]
        public void UnknownState_Equals_UnknownState()
        {
            IComponentState state1 = new UnknownComponentState();
            IComponentState state2 = new UnknownComponentState();

            state1.Equals(state2).ShouldBeEquivalentTo(true);
        }
    }
}
