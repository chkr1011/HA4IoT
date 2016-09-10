using System;
using System.Text;
using HA4IoT.Networking.WebSockets;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Networking.Tests
{
    [TestClass]
    public class WebSocketFrameTests
    {
        [TestMethod]
        public void WebSocketFrame_Simple()
        {
            var payload = new JObject
            {
                ["Hello"] = "World"
            };

            var payloadBuffer = Encoding.UTF8.GetBytes(payload.ToString());
            var webSocketFrame = WebSocketFrame.Create(payloadBuffer);

            var result = webSocketFrame.ToByteArray();
            var expected = Convert.FromBase64String("ghh7DQogICJIZWxsbyI6ICJXb3JsZCINCn0=");

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void WebSocketFrame_LargePayload()
        {
            var payload = new JObject
            {
                ["Hello12121212121212121212121212121212121212121212121212121AAAAAAAAAAAAAAA"] =
                    "World56565656565656565656565656565656565656565656565BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBCCCCCCCCCC"
            };

            var payloadBuffer = Encoding.UTF8.GetBytes(payload.ToString());
            var webSocketFrame = WebSocketFrame.Create(payloadBuffer);

            var result = webSocketFrame.ToByteArray();
            var expected = Convert.FromBase64String("gn4As3sNCiAgIkhlbGxvMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjFBQUFBQUFBQUFBQUFBQUEiOiAiV29ybGQ1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NUJCQkJCQkJCQkJCQkJCQkJCQkJCQkJCQkJCQkJCQkNDQ0NDQ0NDQ0MiDQp9");

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void WebSocketFrame_Parse()
        {
            var payload = new JObject
            {
                ["Hello"] = "World"
            };

            var payloadBuffer = Encoding.UTF8.GetBytes(payload.ToString());
            var sourceWebSocketFrame = WebSocketFrame.Create(payloadBuffer);
            sourceWebSocketFrame.Opcode = WebSocketOpcode.Ping;

            var buffer = sourceWebSocketFrame.ToByteArray();

            var targetWebSocketFrame = WebSocketFrame.Parse(buffer).WebSocketFrame;

            Assert.AreEqual(sourceWebSocketFrame.Fin, targetWebSocketFrame.Fin);
            Assert.AreEqual(sourceWebSocketFrame.Opcode, targetWebSocketFrame.Opcode);
            CollectionAssert.AreEqual(payloadBuffer, targetWebSocketFrame.Payload);
        }
    }
}
