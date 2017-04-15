using System;

namespace HA4IoT.Net.WebSockets
{
    public class WebSocketFrameParseResult
    {
        public WebSocketFrameParseResult(WebSocketFrame webSocketFrame, byte[] overhead)
        {
            if (webSocketFrame == null) throw new ArgumentNullException(nameof(webSocketFrame));
            if (overhead == null) throw new ArgumentNullException(nameof(overhead));

            WebSocketFrame = webSocketFrame;
            Overhead = overhead;
        }

        public WebSocketFrame WebSocketFrame { get; }

        public byte[] Overhead { get; }
    }
}
