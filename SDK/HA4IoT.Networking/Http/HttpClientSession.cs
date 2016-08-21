using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography.Core;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking.Json;
using HA4IoT.Networking.WebSockets;

namespace HA4IoT.Networking.Http
{
    public sealed class HttpClientSession : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private const int REQUEST_BUFFER_SIZE = 16*1024;
        private readonly byte[] _buffer = new byte[REQUEST_BUFFER_SIZE];

        private readonly HttpRequestParser _requestParser = new HttpRequestParser();
        private readonly HttpResponseSerializer _responseSerializer = new HttpResponseSerializer();

        private readonly StreamSocket _client;
        private readonly Stream _inputStream;
        private readonly Stream _outputStream;

        private readonly Func<HttpContext, bool> _httpRequestReceivedCallback;
        private readonly Action<WebSocketContext> _webSocketConnectedCallback;

        private WebSocketClientSession _webSocketClientSession;
        
        public HttpClientSession(
            StreamSocket client, 
            Func<HttpContext, bool> httpRequestReceivedCallback,
            Action<WebSocketContext> webSocketConnectedCallback)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (httpRequestReceivedCallback == null) throw new ArgumentNullException(nameof(httpRequestReceivedCallback));
            if (webSocketConnectedCallback == null) throw new ArgumentNullException(nameof(webSocketConnectedCallback));

            _client = client;
            _inputStream = _client.InputStream.AsStreamForRead(_buffer.Length);
            _outputStream = _client.OutputStream.AsStreamForWrite(REQUEST_BUFFER_SIZE);

            _httpRequestReceivedCallback = httpRequestReceivedCallback;
            _webSocketConnectedCallback = webSocketConnectedCallback;
        }

        public void WaitForData()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_webSocketClientSession != null)
                {
                    _webSocketClientSession.WaitForDataAsync().Wait();
                }
                else
                {
                    HandleHttpRequest();
                }
            }
        }

        private void HandleHttpRequest()
        {
            HttpRequest request = ReceiveHttpRequest();
            if (request == null)
            {
                _cancellationTokenSource.Cancel();
                return;
            }
            
            var context = new HttpContext(request, new HttpResponse());
            PrepareResponseHeaders(context);

            bool isWebSocketRequest = request.Headers.ValueEquals(HttpHeaderNames.Upgrade, "websocket");
            if (isWebSocketRequest)
            {
                UpgradeToWebSocket(context);
            }
            else
            {
                HandleHttpRequest(context);
            }
        }

        private void HandleHttpRequest(HttpContext context)
        {
            ProcessHttpRequest(context);
            SendResponse(context);

            if (context.Request.Headers.GetConnectionMustBeClosed())
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private HttpRequest ReceiveHttpRequest()
        {
            int bufferLength = _inputStream.Read(_buffer, 0, _buffer.Length);
            
            HttpRequest httpRequest;
            if (!_requestParser.TryParse(_buffer, bufferLength, out httpRequest))
            {
                return null;
            }

            if (httpRequest.Headers.GetRequiresContinue() && httpRequest.Headers.GetHasBodyContent())
            {
                bufferLength += _inputStream.Read(_buffer, bufferLength, _buffer.Length - bufferLength);

                if (!_requestParser.TryParse(_buffer, bufferLength, out httpRequest))
                {
                    return null;
                }
            }

            return httpRequest;
        }

        private void ProcessHttpRequest(HttpContext context)
        {
            try
            {
                bool requestHandled = _httpRequestReceivedCallback.Invoke(context);
                if (!requestHandled)
                {
                    context.Response.StatusCode = HttpStatusCode.BadRequest;
                }
            }
            catch (Exception exception)
            {
                if (context != null)
                {
                    context.Response.StatusCode = HttpStatusCode.InternalServerError;
                    context.Response.Body = new JsonBody(exception.ToJsonObject());
                }
            }
        }

        private void PrepareResponseHeaders(HttpContext context)
        {
            context.Response.Headers[HttpHeaderNames.AccessControlAllowOrigin] = "*";

            if (context.Request.Headers.GetConnectionMustBeClosed())
            {
                context.Response.Headers[HttpHeaderNames.Connection] = "close";
            }
        }

        private void SendResponse(HttpContext context)
        {
            byte[] response = _responseSerializer.SerializeResponse(context);
            _outputStream.Write(response, 0, response.Length);
            _outputStream.Flush();
        }

        private void UpgradeToWebSocket(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = HttpStatusCode.SwitchingProtocols;
            httpContext.Response.Headers[HttpHeaderNames.Connection] = "Upgrade";
            httpContext.Response.Headers[HttpHeaderNames.Upgrade] = "websocket";
            httpContext.Response.Headers[HttpHeaderNames.SecWebSocketAccept] = GenerateWebSocketAccept(httpContext);
            //httpContext.Response.Headers[HttpHeaderNames.SecWebSocketProtocol] = "HA4IoT";

            SendResponse(httpContext);

            _webSocketClientSession = new WebSocketClientSession(_client);
            _webSocketConnectedCallback(new WebSocketContext(httpContext.Request, _webSocketClientSession));
        }

        private string GenerateWebSocketAccept(HttpContext httpContext)
        {
            var webSocketKey = httpContext.Request.Headers[HttpHeaderNames.SecWebSocketKey];
            var responseKey = webSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            var responseKeyBuffer = Encoding.ASCII.GetBytes(responseKey).AsBuffer();
            
            var sha1 = HashAlgorithmProvider.OpenAlgorithm("SHA1");
            var sha1Buffer = sha1.HashData(responseKeyBuffer);
            
            return Convert.ToBase64String(sha1Buffer.ToArray());
        }

        public void Dispose()
        {
            _inputStream.Dispose();
            _outputStream.Dispose();
            _client.Dispose();
        }
    }
}