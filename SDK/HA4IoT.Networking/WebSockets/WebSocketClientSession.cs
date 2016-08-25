using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking.WebSockets;
using Buffer = Windows.Storage.Streams.Buffer;

namespace HA4IoT.Networking.WebSockets
{
    public class WebSocketClientSession : IWebSocketClientSession
    {
        private const int RequestBufferSize = 16 * 1024;

        private readonly Guid _sessionUid = Guid.NewGuid();
        private readonly List<WebSocketFrame> _frameQueue = new List<WebSocketFrame>();
        private readonly StreamSocket _clientSocket;

        public WebSocketClientSession(StreamSocket clientSocket)
        {
            if (clientSocket == null) throw new ArgumentNullException(nameof(clientSocket));

            _clientSocket = clientSocket;

            Log.Verbose($"WebSocket session '{_sessionUid}' created.");
        }

        public event EventHandler<WebSocketMessageReceivedEventArgs> MessageReceived;

        public event EventHandler Closed;

        public async Task<WebSocketFrame> WaitForFrameAsync()
        {
            var buffer = new Buffer(RequestBufferSize);
            var data = await _clientSocket.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);

            if (data == null || data.Length == 0)
            {
                Log.Verbose($"WebSocket session '{_sessionUid}' received no data.");
                return null;
            }

            var webSocketFrame = WebSocketFrame.FromByteArray(data.ToArray());
            Log.Verbose($"WebSocket session '{_sessionUid}' received '{webSocketFrame.Opcode}' from client.");

            switch (webSocketFrame.Opcode)
            {
                case WebSocketOpcode.Ping:
                    {
                        webSocketFrame.Opcode = WebSocketOpcode.Pong;
                        await SendAsync(webSocketFrame);

                        return webSocketFrame;
                    }

                case WebSocketOpcode.ConnectionClose:
                    {
                        Closed?.Invoke(this, EventArgs.Empty);
                        return webSocketFrame;
                    }

                case WebSocketOpcode.Pong:
                    {
                        return webSocketFrame;
                    }
            }

            _frameQueue.Add(webSocketFrame);

            if (webSocketFrame.Fin)
            {
                var message = GenerateMessage();
                _frameQueue.Clear();

                MessageReceived?.Invoke(this, new WebSocketMessageReceivedEventArgs(message, this));
            }

            return webSocketFrame;
        }

        public async Task CloseAsync()
        {
            await _clientSocket.CancelIOAsync();
            _clientSocket.Dispose();

            Log.Verbose($"WebSocket session '{_sessionUid}' closed.");
        }

        public async Task SendAsync(JsonObject json)
        {
            await SendAsync(json.ToString());
        }

        public async Task SendAsync(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            await SendAsync(WebSocketFrame.Create(text));
        }

        public async Task SendAsync(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            await SendAsync(WebSocketFrame.Create(data));
        }

        private WebSocketMessage GenerateMessage()
        {
            ValidateFrameQueue();

            var buffer = new List<byte>();
            foreach (var frame in _frameQueue)
            {
                buffer.AddRange(frame.Payload);
            }

            var messageType = _frameQueue.First().Opcode;

            if (messageType == WebSocketOpcode.Text)
            {
                return new WebSocketTextMessage(Encoding.UTF8.GetString(buffer.ToArray()));
            }

            if (messageType == WebSocketOpcode.Binary)
            {
                return new WebSocketBinaryMessage(buffer.ToArray());
            }

            throw new NotSupportedException();
        }

        private void ValidateFrameQueue()
        {
            // Details: https://tools.ietf.org/html/rfc6455#section-5.6 PAGE 34
            if (!_frameQueue.Last().Fin)
            {
                throw new InvalidOperationException("Fragmented frames are invalid.");
            }

            if (_frameQueue.First().Opcode != WebSocketOpcode.Binary &&
                _frameQueue.First().Opcode != WebSocketOpcode.Text)
            {
                throw new InvalidOperationException("Frame opcode is invalid.");
            }

            if (_frameQueue.Count > 2)
            {
                for (int i = 1; i < _frameQueue.Count - 1; i++)
                {
                    if (_frameQueue[i].Opcode != WebSocketOpcode.Continuation)
                    {
                        throw new InvalidOperationException("Fragmented frame is invalid.");
                    }

                    if (_frameQueue[i].Fin)
                    {
                        throw new InvalidOperationException("Fragmented frame is invalid.");
                    }
                }
            }
        }

        private async Task SendAsync(WebSocketFrame frame)
        {
            var frameBuffer = frame.ToByteArray().AsBuffer();

            await _clientSocket.OutputStream.WriteAsync(frameBuffer);
            await _clientSocket.OutputStream.FlushAsync();

            Log.Verbose($"WebSocket session '{_sessionUid}' sent {frame.Opcode} to client.");
        }
    }
}
