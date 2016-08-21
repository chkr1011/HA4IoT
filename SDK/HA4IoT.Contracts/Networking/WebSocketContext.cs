using System;
using System.Text;
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

        public void Send(JsonObject data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            
            Send(data.ToString());
        }

        public void Send(string data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Send(Encoding.UTF8.GetBytes(data));
        }

        public void Send(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _webSocketClientSession.SendAsync(data);
        }

        public void Close()
        {
            // TODO: Implement
        }
    }
}
