using FluentAssertions;
using HA4IoT.Contracts.Components;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Components.Tests
{
    [TestClass]
    public class ComponentComparisonStateTests
    {
        [TestMethod]
        public void NumericState_Equals_NumericState()
        {
            var state1 = new ComponentState(5);
            var state2 = new ComponentState(5);

            state1.Equals(state2).ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void NumericState_NotEquals_NumericState()
        {
            var state1 = new ComponentState(5);
            var state2 = new ComponentState(6);

            state1.Equals(state2).ShouldBeEquivalentTo(false);
        }

        [TestMethod]
        public void State_Equals_State()
        {
            var state1 = new ComponentState("Off");
            var state2 = new ComponentState("Off");

            state1.Equals(state2).ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void State_NotEquals_State()
        {
            var state1 = new ComponentState("Off");
            var state2 = new ComponentState("On");

            state1.Equals(state2).ShouldBeEquivalentTo(false);
        }

        [TestMethod]
        public void State_NotEquals_DifferentState()
        {
            var state1 = new ComponentState("Off");
            var state2 = new ComponentState(5);

            state1.Equals(state2).ShouldBeEquivalentTo(false);
        }

        [TestMethod]
        public void UnknownState_NotEquals_DifferentState()
        {
            var state1 = new ComponentState(null);
            var state2 = new ComponentState(5);

            state1.Equals(state2).ShouldBeEquivalentTo(false);
        }

        [TestMethod]
        public void UnknownState_Equals_UnknownState()
        {
            var state1 = new ComponentState(null);
            var state2 = new ComponentState(null);

            state1.Equals(state2).ShouldBeEquivalentTo(true);
        }
    }
}
