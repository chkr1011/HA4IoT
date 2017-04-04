using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Logging;
using Newtonsoft.Json.Linq;
using Buffer = Windows.Storage.Streams.Buffer;

namespace HA4IoT.Networking.WebSockets
{
    public class WebSocketClientSession : IWebSocketClientSession
    {
        private const int RequestBufferSize = 16 * 1024;

        private readonly Guid _sessionUid = Guid.NewGuid();
        private readonly List<WebSocketFrame> _frameQueue = new List<WebSocketFrame>();
        private readonly StreamSocket _clientSocket;
        private readonly ILogger _log;

        private byte[] _overhead = new byte[0];

        public WebSocketClientSession(StreamSocket clientSocket, ILogger log)
        {
            if (clientSocket == null) throw new ArgumentNullException(nameof(clientSocket));
            if (log == null) throw new ArgumentNullException(nameof(log));

            _clientSocket = clientSocket;
            _log = log;

            _log.Verbose($"WebSocket session '{_sessionUid}' created.");
        }

        public event EventHandler<WebSocketMessageReceivedEventArgs> MessageReceived;

        public event EventHandler Closed;

        public async Task WaitForFrameAsync()
        {
            var buffer = new Buffer(RequestBufferSize);
            await _clientSocket.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);

            var data = new List<byte>(_overhead);
            _overhead = new byte[0];
            data.AddRange(buffer.ToArray());

            var parseWebSocketFrameResult = WebSocketFrame.Parse(data.ToArray());
            if (parseWebSocketFrameResult.WebSocketFrame == null)
            {
                _overhead = parseWebSocketFrameResult.Overhead;
                return;
            }

            var webSocketFrame = parseWebSocketFrameResult.WebSocketFrame;
            switch (webSocketFrame.Opcode)
            {
                case WebSocketOpcode.Ping:
                    {
                        webSocketFrame.Opcode = WebSocketOpcode.Pong;
                        await SendAsync(webSocketFrame);

                        return;
                    }

                case WebSocketOpcode.ConnectionClose:
                    {
                        CloseAsync().Wait();
                        return;
                    }

                case WebSocketOpcode.Pong:
                    {
                        return;
                    }
            }

            _frameQueue.Add(webSocketFrame);

            if (webSocketFrame.Fin)
            {
                var message = GenerateMessage();
                _frameQueue.Clear();

                MessageReceived?.Invoke(this, new WebSocketMessageReceivedEventArgs(message, this));
            }
        }

        public async Task CloseAsync()
        {
            await _clientSocket.CancelIOAsync();
            Closed?.Invoke(this, EventArgs.Empty);

            _log.Verbose($"WebSocket session '{_sessionUid}' closed.");
        }

        public async Task SendAsync(JObject json)
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
        }
    }
}
