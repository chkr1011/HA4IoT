using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace HA4IoT.Contracts.Networking
{
    public class WebSocketContext
    {
        private readonly IWebSocketClientSession _webSocketClientSession;

        public WebSocketContext(HttpRequest httpRequest, IWebSocketClientSession webSocketClientSession)
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (webSocketClientSession == null) throw new ArgumentNullException(nameof(webSocketClientSession));

            HttpRequest = httpRequest;
            _webSocketClientSession = webSocketClientSession;
        }

        public HttpRequest HttpRequest { get; }

        public void SendFrame(JsonObject data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var buffer = Encoding.UTF8.GetBytes(data.ToString());
            SendFrame(buffer);
        }

        public void SendFrame(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _webSocketClientSession.SendAsync(data);
        }
    }
}
