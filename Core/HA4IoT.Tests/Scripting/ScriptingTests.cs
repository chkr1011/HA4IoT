using HA4IoT.Contracts.Scripting;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Scripting
{
    [TestClass]
    public class ScriptingTests
    {
        [TestMethod]
        public void Scripting_Return_Boolean()
        {
            var testController = new TestController();
            var scriptingService = testController.GetInstance<IScriptingService>();

            var session = scriptingService.CreateScriptingSession("return true");
            var result = session.Execute();

            Assert.IsTrue(result != null, "Result is null");
            Assert.IsTrue(result.Exception == null, "Exception is not null");
            Assert.IsTrue((bool)result.Value, "Inner result is invalid");
        }

        [TestMethod]
        public void Scripting_Return_Integer()
        {
            var testController = new TestController();
            var scriptingService = testController.GetInstance<IScriptingService>();

            var session = scriptingService.CreateScriptingSession("return 1+3");
            var result = session.Execute();

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Exception == null);
            Assert.AreEqual(4D, result.Value);
        }

        [TestMethod]
        public void Scripting_Return_String()
        {
            var testController = new TestController();
            var scriptingService = testController.GetInstance<IScriptingService>();

            var session = scriptingService.CreateScriptingSession("return 'Hallo' .. ' Welt'");
            var result = session.Execute();

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Exception == null);
            Assert.AreEqual("Hallo Welt", result.Value);
        }

        [TestMethod]
        public void Scripting_Call_Function()
        {
            var testController = new TestController();
            var scriptingService = testController.GetInstance<IScriptingService>();

            var session = scriptingService.CreateScriptingSession("function test() return 1 end return 0");
            var result = session.Execute("test");

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Exception == null);
            Assert.AreEqual(1D, result.Value);
        }

        [TestMethod]
        public void Scripting_Dont_Call_Function()
        {
            var testController = new TestController();
            var scriptingService = testController.GetInstance<IScriptingService>();

            var session = scriptingService.CreateScriptingSession("function test() return 1 end return 0");
            var result = session.Execute();

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Exception == null);
            Assert.AreEqual(0D, result.Value);
        }

        [TestMethod]
        public void Scripting_Multiple_Calls_In_Same_Session()
        {
            var testController = new TestController();
            var scriptingService = testController.GetInstance<IScriptingService>();

            var session = scriptingService.CreateScriptingSession("function first() x = 2 return 11 end function second() x = x + 1 return 22 end return x");

            var result = session.Execute();
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Exception == null);
            Assert.AreEqual(null, result.Value);

            result = session.Execute("first");
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Exception == null);
            Assert.AreEqual(11D, result.Value);

            result = session.Execute();
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Exception == null);
            Assert.AreEqual(2D, result.Value);

            result = session.Execute("second");
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Exception == null);
            Assert.AreEqual(22D, result.Value);

            result = session.Execute();
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Exception == null);
            Assert.AreEqual(3D, result.Value);
        }
    }
}
