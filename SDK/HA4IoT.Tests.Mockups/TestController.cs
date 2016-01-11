using HA4IoT.Core;

namespace HA4IoT.Tests.Mockups
{
    public class TestController : ControllerBase
    {
        public TestController()
        {
            Logger = new TestNotificationHandler();
            HttpApiController = new TestHttpRequestController();
        }
    }
}
