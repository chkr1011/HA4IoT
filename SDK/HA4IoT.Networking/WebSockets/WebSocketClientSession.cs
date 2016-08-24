using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Networking;
using Buffer = Windows.Storage.Streams.Buffer;

namespace HA4IoT.Networking.WebSockets
{
    public class WebSocketClientSession : IWebSocketClientSession
    {
        private const int RequestBufferSize = 16 * 1024;

        private readonly StreamSocket _clientSocket;

        public WebSocketClientSession(StreamSocket clientSocket)
        {
            if (clientSocket == null) throw new ArgumentNullException(nameof(clientSocket));

            _clientSocket = clientSocket;
        }

        public async Task<WebSocketFrame> WaitForFrame()
        {
            var buffer = new Buffer(RequestBufferSize);
            var data = await _clientSocket.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);
            
            var webSocketFrame = WebSocketFrame.FromByteArray(data.ToArray());
            if (webSocketFrame.Opcode == WebSocketOpcode.Ping)
            {
                webSocketFrame.Opcode = WebSocketOpcode.Pong;
                await SendAsync(webSocketFrame);

                return webSocketFrame;
            }

            if (webSocketFrame.Opcode == WebSocketOpcode.ConnectionClose)
            {
                return webSocketFrame;
            }

            // TODO: Handle request.
            return webSocketFrame;
        }

        public async Task SendAsync(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var frame = WebSocketFrame.Create(data);
            await SendAsync(frame);
        }

        private async Task SendAsync(WebSocketFrame frame)
        {
            var frameBuffer = frame.ToByteArray().AsBuffer();

            await _clientSocket.OutputStream.WriteAsync(frameBuffer);
            await _clientSocket.OutputStream.FlushAsync();
        }
    }
}
