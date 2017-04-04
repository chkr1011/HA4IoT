using System;

namespace HA4IoT.Networking.WebSockets
{
    public class WebSocketMessageReceivedEventArgs : EventArgs
    {
        public WebSocketMessageReceivedEventArgs(WebSocketMessage message, IWebSocketClientSession webSocketClientSession)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (webSocketClientSession == null) throw new ArgumentNullException(nameof(webSocketClientSession));

            Message = message;
            WebSocketClientSession = webSocketClientSession;
        }

        public IWebSocketClientSession WebSocketClientSession { get; }

        public WebSocketMessage Message { get; }
    }
}
