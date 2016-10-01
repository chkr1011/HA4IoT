using System;
using System.Linq;
using FluentAssertions;
using HA4IoT.Contracts.Networking.Http;
using HA4IoT.Networking.Http;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Networking.Tests
{
    [TestClass]
    public class HttpResponseSerializerTests
    {
        [TestMethod]
        public void Serialize_HttpRequest()
        {
            var request = new HttpRequest(HttpMethod.Get, "", new Version(1, 1), "", new HttpHeaderCollection(), "", 0);

            var response = new HttpResponse();
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Body = new PlainTextBody().WithContent("{\"text\":1234}");
            response.Headers["A"] = 1.ToString();
            response.Headers["B"] = "x";

            var serializer = new HttpResponseSerializer();
            byte[] buffer = serializer.SerializeResponse(new HttpContext(request, response));
            byte[] requiredBuffer = { 72,84,84,80,47,49,46,49,32,52,48,48,32,66,97,100,32,82,101,113,117,101,115,116,13,10,65,58,49,13,10,66,58,120,13,10,67,111,110,116,101,110,116,45,84,121,112,101,58,116,101,120,116,47,112,108,97,105,110,59,32,99,104,97,114,115,101,116,61,117,116,102,45,56,13,10,67,111,110,116,101,110,116,45,76,101,110,103,116,104,58,49,51,13,10,13,10,123,34,116,101,120,116,34,58,49,50,51,52,125};

            bool matching = buffer.SequenceEqual(requiredBuffer);
            matching.ShouldBeEquivalentTo(true);
        }
    }
}
