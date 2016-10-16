using System;

namespace HA4IoT.Contracts.Networking.WebSockets
{
    public class WebSocketBinaryMessage : WebSocketMessage
    {
        public WebSocketBinaryMessage(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Data = data;
        }

        public byte[] Data { get; }
    }
}
