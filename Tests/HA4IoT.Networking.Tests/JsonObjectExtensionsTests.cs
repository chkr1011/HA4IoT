using Windows.Data.Json;
using HA4IoT.Networking.Json;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Networking.Tests
{
    [TestClass]
    public class JsonObjectExtensionsTests
    {
        [TestMethod]
        public void JsonImport_Flat()
        {
            var source = new JsonObject();
            source.SetValue("B", 1);
            source.SetValue("C", 2);

            var target = new JsonObject();
            target.SetValue("A", 0);

            target.Import(source);

            string result = target.ToString();
            Assert.AreEqual("{\"A\":0,\"C\":2,\"B\":1}", result);
        }

        [TestMethod]
        public void JsonImport_ExistingRoot()
        {
            var target = new JsonObject();
            target.SetValue("A", new JsonObject().WithNumber("X", 0).WithNumber("Y", 1));

            var source = new JsonObject();
            source.SetValue("A", new JsonObject().WithNumber("Z", 2));

            target.Import(source);

            string result = target.ToString();
            Assert.AreEqual("{\"A\":{\"X\":0,\"Y\":1,\"Z\":2}}", result);
        }

        [TestMethod]
        public void JsonImport_ExistingPath()
        {
            var tX = new JsonObject();
            tX.SetValue("P", 99);

            var tA = new JsonObject();
            tA.SetNamedValue("X", tX);

            var target = new JsonObject();
            target.SetNamedValue("A", tA);
            
            var sX = new JsonObject();
            sX.SetValue("Q", 100);

            var sA = new JsonObject();
            sA.SetNamedValue("X", sX);
            
            var source = new JsonObject();
            source.SetNamedValue("A", sA);

            target.Import(source);

            string result = target.ToString();
            Assert.AreEqual("{\"A\":{\"X\":{\"P\":99,\"Q\":100}}}", result);
        }

        [TestMethod]
        public void JsonImport_ExistingValue()
        {
            var source = new JsonObject();
            source.SetValue("A", 0);
            
            var target = new JsonObject();
            target.SetValue("A", 1);

            target.Import(source);

            string result = target.ToString();
            Assert.AreEqual("{\"A\":0}", result);
        }

        [TestMethod]
        public void JsonImport_ExistingValueWithDifferentType()
        {
            var source = new JsonObject();
            source.SetValue("A", false);

            var target = new JsonObject();
            target.SetValue("A", 1);
            
            Assert.ThrowsException<ImportNotPossibleException>(() => target.Import(source));
        }
    }
}
