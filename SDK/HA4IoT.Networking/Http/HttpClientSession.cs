using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography.Core;
using HA4IoT.Contracts.Networking.Http;
using HA4IoT.Networking.Json;

namespace HA4IoT.Networking.Http
{
    public sealed class HttpClientSession : IDisposable
    {
        private const int RequestBufferSize = 1024*1024; // 1 MB

        private readonly Version _supportedHttpVersion = new Version(1, 1);
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly byte[] _buffer = new byte[RequestBufferSize];

        private readonly HttpRequestParser _requestParser = new HttpRequestParser();
        private readonly HttpResponseSerializer _responseSerializer = new HttpResponseSerializer();

        private readonly StreamSocket _client;
        private readonly Stream _inputStream;
        private readonly Action<HttpRequestReceivedEventArgs> _httpRequestReceivedCallback;
        private readonly Action<UpgradedToWebSocketSessionEventArgs> _upgradeToWebSocketSessionCallback;

        public HttpClientSession(
            StreamSocket client,
            CancellationTokenSource cancellationTokenSource,
            Action<HttpRequestReceivedEventArgs> httpRequestReceivedCallback,
            Action<UpgradedToWebSocketSessionEventArgs> upgradeToWebSocketSessionCallback)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (httpRequestReceivedCallback == null)
                throw new ArgumentNullException(nameof(httpRequestReceivedCallback));
            if (upgradeToWebSocketSessionCallback == null)
                throw new ArgumentNullException(nameof(upgradeToWebSocketSessionCallback));

            _client = client;
            _inputStream = client.InputStream.AsStreamForRead(RequestBufferSize);

            _cancellationTokenSource = cancellationTokenSource;

            _httpRequestReceivedCallback = httpRequestReceivedCallback;
            _upgradeToWebSocketSessionCallback = upgradeToWebSocketSessionCallback;
        }

        public void WaitForRequest()
        {
            HttpRequest request;
            if (!TryReceiveHttpRequest(out request))
            {
                _cancellationTokenSource.Cancel();
                return;
            }
            
            var context = new HttpContext(request, new HttpResponse());
            PrepareResponseHeaders(context);

            if (context.Request.HttpVersion != _supportedHttpVersion)
            {
                context.Response.StatusCode = HttpStatusCode.HttpVersionNotSupported;
                SendResponse(context);

                _cancellationTokenSource.Cancel();
                return;
            }

            var isWebSocketRequest = request.Headers.ValueEquals(HttpHeaderNames.Upgrade, "websocket");
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

            if (context.Response.Headers.GetConnectionMustBeClosed())
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private bool TryReceiveHttpRequest(out HttpRequest httpRequest)
        {
            httpRequest = null;

            try
            {
                var receivedBytes = _inputStream.Read(_buffer, 0, _buffer.Length);
                if (receivedBytes == 0)
                {
                    return false;
                }

                if (!_requestParser.TryParse(_buffer, receivedBytes, out httpRequest))
                {
                    return false;
                }

                while (httpRequest.GetRequiresContinue())
                {
                    var additionalReceivedBytes = _inputStream.Read(_buffer, 0, _buffer.Length);
                    if (additionalReceivedBytes == 0)
                    {
                        return false;
                    }

                    receivedBytes += additionalReceivedBytes;

                    if (!_requestParser.TryParse(_buffer, receivedBytes, out httpRequest))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private void ProcessHttpRequest(HttpContext context)
        {
            try
            {
                var eventArgs = new HttpRequestReceivedEventArgs(context);
                _httpRequestReceivedCallback(eventArgs);

                if (!eventArgs.IsHandled)
                {
                    context.Response.StatusCode = HttpStatusCode.NotFound;
                }
            }
            catch (Exception exception)
            {
                if (context != null)
                {
                    context.Response.StatusCode = HttpStatusCode.InternalServerError;
                    context.Response.Body = new JsonBody(JsonSerializer.SerializeException(exception));
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

        private async void SendResponse(HttpContext context)
        {
            try
            {
                var response = _responseSerializer.SerializeResponse(context);

                await _client.OutputStream.WriteAsync(response.AsBuffer());
                await _client.OutputStream.FlushAsync();
            }
            catch (IOException)
            {  
            }
        }

        private void UpgradeToWebSocket(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = HttpStatusCode.SwitchingProtocols;
            httpContext.Response.Headers[HttpHeaderNames.Connection] = "Upgrade";
            httpContext.Response.Headers[HttpHeaderNames.Upgrade] = "websocket";
            httpContext.Response.Headers[HttpHeaderNames.SecWebSocketAccept] = GenerateWebSocketAccept(httpContext);
            
            SendResponse(httpContext);
            _upgradeToWebSocketSessionCallback(new UpgradedToWebSocketSessionEventArgs(httpContext.Request));
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
            _client.Dispose();
        }
    }
}