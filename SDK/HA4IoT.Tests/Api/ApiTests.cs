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

        [TestMethod]
        public void API_GenericInvoke()
        {
            var testController = new TestController();

            var parameter = new JObject
            {
                ["Hello"] = "World"
            };

            var apiRequest = new ApiRequest
            {
                Action = "Ping",
                Parameter = parameter
            };

            var apiContext = testController.InvokeApi("Execute", JObject.FromObject(apiRequest));
            Assert.AreEqual(ApiResultCode.Success, apiContext.ResultCode);
            Assert.AreEqual("World", apiContext.Result["Hello"]);
        }

        [TestMethod]
        public void API_UseHash()
        {
            var testController = new TestController();

            var parameter = new JObject
            {
                ["Hello"] = "World"
            };

            var apiRequest = new ApiRequest
            {
                Action = "Ping",
                Parameter = parameter,
                ResultHash = ""
            };

            var apiContext = testController.InvokeApi("Execute", JObject.FromObject(apiRequest));
            Assert.AreEqual(ApiResultCode.Success, apiContext.ResultCode);
            Assert.AreEqual("World", apiContext.Result["Hello"]);

            // Repeat same request with hash!

            apiRequest.ResultHash = apiContext.ResultHash;
            apiContext = testController.InvokeApi("Execute", JObject.FromObject(apiRequest));

            Assert.AreEqual(ApiResultCode.Success, apiContext.ResultCode);
            Assert.AreEqual(apiContext.ResultHash, apiRequest.ResultHash);

            // Result should be empty to decrease size.
            Assert.AreEqual(apiContext.Result.ToString(), new JObject().ToString());
        }
    }
}
