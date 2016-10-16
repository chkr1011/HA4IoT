using HA4IoT.Contracts.Components;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Tests.Components
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void NumericValue_Serialize()
        {
            var state = new ComponentState(5F);
            var jsonValue = state.JToken;

            Assert.AreEqual(JTokenType.Float, jsonValue.Type);
            Assert.AreEqual(5, jsonValue.ToObject<int>());
        }

        [TestMethod]
        public void StatefulState_Serialize()
        {
            var state = new ComponentState("Off");
            var jsonValue = state.JToken;

            Assert.AreEqual(JTokenType.String, jsonValue.Type);
            Assert.AreEqual("Off", jsonValue.ToObject<string>());
        }

        [TestMethod]
        public void UnknownState_Serialize()
        {
            var state = new ComponentState(null);
            var jsonValue = state.JToken;

            Assert.AreEqual(JTokenType.Null, jsonValue.Type);
            Assert.IsNull(jsonValue.ToObject<object>());
        }
    }
}
