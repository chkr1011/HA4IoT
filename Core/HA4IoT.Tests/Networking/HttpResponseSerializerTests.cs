using System;
using System.Linq;
using System.Net;
using HA4IoT.Networking.Http;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Networking
{
    [TestClass]
    public class HttpResponseSerializerTests
    {
        [TestMethod]
        public void Http_SerializeHttpRequest()
        {
            var request = new HttpRequest();
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Body = new PlainTextBody {Content = "{\"text\":1234}"}
            };

            response.Headers["A"] = 1.ToString();
            response.Headers["B"] = "x";

            var serializer = new HttpResponseWriter();
            var buffer = serializer.SerializeResponse(new HttpContext(request, response));
            var requiredBuffer = Convert.FromBase64String("SFRUUC8xLjEgNDAwIEJhZFJlcXVlc3QNCkE6MQ0KQjp4DQpDb250ZW50LVR5cGU6dGV4dC9wbGFpbjsgY2hhcnNldD11dGYtOA0KQ29udGVudC1MZW5ndGg6MTMNCg0KeyJ0ZXh0IjoxMjM0fQ==");

            Assert.IsTrue(buffer.SequenceEqual(requiredBuffer));
        }
    }
}
