using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Networking;
using Buffer = Windows.Storage.Streams.Buffer;

namespace HA4IoT.Networking.WebSockets
{
    public class WebSocketClientSession : IWebSocketClientSession
    {
        private readonly StreamSocket _clientSocket;

        public WebSocketClientSession(StreamSocket clientSocket)
        {
            if (clientSocket == null) throw new ArgumentNullException(nameof(clientSocket));

            _clientSocket = clientSocket;
        }

        public async Task WaitForDataAsync()
        {
            var buffer = new Buffer(16 * 1024);
            var data = await _clientSocket.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);
            
            var webSocketFrame = WebSocketFrame.FromByteArray(data.ToArray());
            if (webSocketFrame.Opcode == WebSocketOpcode.Ping)
            {
                webSocketFrame.Opcode = WebSocketOpcode.Pong;
                await SendAsync(webSocketFrame);

                return;
            }

            // TODO: Handle request.
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
