using System;
using System.Text;
using Windows.Data.Json;
using HA4IoT.Networking.Http;
using HA4IoT.Networking.Json;
using HA4IoT.Networking.WebSockets;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Networking.Tests
{
    [TestClass]
    public class WebSocketFrameTests
    {
        [TestMethod]
        public void WebSocketFrame_Simple()
        {
            var payload = new JsonObject().WithString("Hello", "World").ToString();
            var payloadBuffer = Encoding.UTF8.GetBytes(payload);
            var webSocketFrame = WebSocketFrame.Create(payloadBuffer);

            var result = webSocketFrame.ToByteArray();
            var expected = Convert.FromBase64String("gRF7IkhlbGxvIjoiV29ybGQifQ==");

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void WebSocketFrame_LargePayload()
        {
            var payload = new JsonObject().WithString("Hello12121212121212121212121212121212121212121212121212121AAAAAAAAAAAAAAA", "World56565656565656565656565656565656565656565656565BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBCCCCCCCCCC").ToString();
            var payloadBuffer = Encoding.UTF8.GetBytes(payload);
            var webSocketFrame = WebSocketFrame.Create(payloadBuffer);

            var result = webSocketFrame.ToByteArray();
            var expected = Convert.FromBase64String("gX4ArHsiSGVsbG8xMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMUFBQUFBQUFBQUFBQUFBQSI6IldvcmxkNTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjVCQkJCQkJCQkJCQkJCQkJCQkJCQkJCQkJCQkJCQkJDQ0NDQ0NDQ0NDIn0=");

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void WebSocketFrame_Parse()
        {
            var payload = new JsonObject().WithString("Hello", "World").ToString();
            var payloadBuffer = Encoding.UTF8.GetBytes(payload);
            var sourceWebSocketFrame = WebSocketFrame.Create(payloadBuffer);
            sourceWebSocketFrame.Opcode = WebSocketOpcode.Ping;

            var buffer = sourceWebSocketFrame.ToByteArray();

            var targetWebSocketFrame = WebSocketFrame.FromByteArray(buffer);

            Assert.AreEqual(sourceWebSocketFrame.Fin, targetWebSocketFrame.Fin);
            Assert.AreEqual(sourceWebSocketFrame.Opcode, targetWebSocketFrame.Opcode);
        }
    }
}
