using Windows.Data.Json;
using FluentAssertions;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Components.Tests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void NumericValue_Serialize()
        {
            IComponentState state = new NumericSensorValue(5);
            IJsonValue jsonValue = state.ToJsonValue();

            jsonValue.ValueType.ShouldBeEquivalentTo(JsonValueType.Number);
            jsonValue.GetNumber().ShouldBeEquivalentTo(5);
        }

        [TestMethod]
        public void StatefulState_Serialize()
        {
            IComponentState state = new StatefulComponentState("Off");
            IJsonValue jsonValue = state.ToJsonValue();

            jsonValue.ValueType.ShouldBeEquivalentTo(JsonValueType.String);
            jsonValue.GetString().ShouldBeEquivalentTo("Off");
        }

        [TestMethod]
        public void UnknownState_Serialize()
        {
            IComponentState state = new UnknownComponentState();
            IJsonValue jsonValue = state.ToJsonValue();

            jsonValue.ValueType.ShouldBeEquivalentTo(JsonValueType.Null);
        }
    }
}
