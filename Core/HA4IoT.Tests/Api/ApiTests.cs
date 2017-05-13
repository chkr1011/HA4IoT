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
            
            var apiCall = testController.InvokeApi("XXX", new JObject());
            Assert.AreEqual(ApiResultCode.ActionNotSupported, apiCall.ResultCode);
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

            var apiCall = testController.InvokeApi("Execute", JObject.FromObject(apiRequest));
            Assert.AreEqual(ApiResultCode.Success, apiCall.ResultCode);
            Assert.AreEqual("World", apiCall.Result["Hello"]);
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

            var apiCall = testController.InvokeApi("Execute", JObject.FromObject(apiRequest));
            Assert.AreEqual(ApiResultCode.Success, apiCall.ResultCode);
            Assert.AreEqual("World", apiCall.Result["Hello"]);

            // Repeat same request with hash!

            apiRequest.ResultHash = apiCall.ResultHash;
            apiCall = testController.InvokeApi("Execute", JObject.FromObject(apiRequest));

            Assert.AreEqual(ApiResultCode.Success, apiCall.ResultCode);
            Assert.AreEqual(apiCall.ResultHash, apiRequest.ResultHash);

            // Result should be empty to decrease size.
            Assert.AreEqual(apiCall.Result.ToString(), new JObject().ToString());
        }
    }
}
