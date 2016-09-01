using Windows.Data.Json;
using FluentAssertions;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Components.Tests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void NumericValue_Serialize()
        {
            var state = new NumericSensorValue(5F);
            var jsonValue = state.ToJsonValue();

            Assert.AreEqual(JTokenType.Float, jsonValue.Type);
            Assert.AreEqual(5, jsonValue.ToObject<int>());
        }

        [TestMethod]
        public void StatefulState_Serialize()
        {
            var state = new NamedComponentState("Off");
            var jsonValue = state.ToJsonValue();

            Assert.AreEqual(JTokenType.String, jsonValue.Type);
            Assert.AreEqual("Off", jsonValue.ToObject<string>());
        }

        [TestMethod]
        public void UnknownState_Serialize()
        {
            var state = new UnknownComponentState();
            var jsonValue = state.ToJsonValue();

            Assert.AreEqual(JTokenType.Null, jsonValue.Type);
            Assert.IsNull(jsonValue.ToObject<object>());
        }
    }
}
