using HA4IoT.Contracts.Api;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Tests.Api
{
    [TestClass]
    public class ApiTests
    {
        [TestMethod]
        public void API_InvokeWrongAction()
        {
            var testController = new TestController();
            
            var apiContext = testController.InvokeApi("XXX", new JObject());
            Assert.AreEqual(ApiResultCode.ActionNotSupported, apiContext.ResultCode);
        }
    }
}
