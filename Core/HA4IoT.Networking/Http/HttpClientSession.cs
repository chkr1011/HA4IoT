using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography.Core;
using Windows.Web.Http;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Networking.Http
{
    public sealed class HttpClientSession : IDisposable
    {
        private readonly Version _supportedHttpVersion = new Version(1, 1);
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly HttpResponseSerializer _responseWriter = new HttpResponseSerializer();
        private readonly HttpRequestReader _httpRequestReader;

        private readonly StreamSocket _client;
        private readonly Stream _inputStream;
        private readonly Stream _outputStream;

        private readonly Action<HttpRequestReceivedEventArgs> _httpRequestReceivedCallback;
        private readonly Action<UpgradedToWebSocketSessionEventArgs> _upgradeToWebSocketSessionCallback;
        private readonly ILogger _log;

        public HttpClientSession(
            StreamSocket client,
            CancellationTokenSource cancellationTokenSource,
            Action<HttpRequestReceivedEventArgs> httpRequestReceivedCallback,
            Action<UpgradedToWebSocketSessionEventArgs> upgradeToWebSocketSessionCallback,
            ILogger log)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));

            _inputStream = client.InputStream.AsStreamForRead();
            _httpRequestReader = new HttpRequestReader(_inputStream);

            _outputStream = client.OutputStream.AsStreamForWrite();

            _cancellationTokenSource = cancellationTokenSource;

            _httpRequestReceivedCallback = httpRequestReceivedCallback ?? throw new ArgumentNullException(nameof(httpRequestReceivedCallback));
            _upgradeToWebSocketSessionCallback = upgradeToWebSocketSessionCallback ?? throw new ArgumentNullException(nameof(upgradeToWebSocketSessionCallback));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task WaitForRequestAsync()
        {
            var httpRequest = await _httpRequestReader.TryReadAsync();
            if (httpRequest == null)
            {
                _cancellationTokenSource.Cancel();
                return;
            }

            var context = new HttpContext(httpRequest, new HttpResponse());
            PrepareResponseHeaders(context);

            if (context.Request.HttpVersion != _supportedHttpVersion)
            {
                context.Response.StatusCode = HttpStatusCode.HttpVersionNotSupported;
                await SendResponseAsync(context);

                _cancellationTokenSource.Cancel();
                return;
            }

            var isWebSocketRequest = httpRequest.Headers.ValueEquals(HttpHeaderName.Upgrade, "websocket");
            if (isWebSocketRequest)
            {
                await UpgradeToWebSocketAsync(context);
            }
            else
            {
                await HandleHttpRequestAsync(context);
            }
        }

        private async Task HandleHttpRequestAsync(HttpContext context)
        {
            ProcessHttpRequest(context);
            await SendResponseAsync(context);

            if (context.Response.Headers.ConnectionMustBeClosed())
            {
                _cancellationTokenSource.Cancel();
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
            catch (Exception)
            {
                if (context != null)
                {
                    context.Response.StatusCode = HttpStatusCode.InternalServerError;
                }
            }
        }

        private void PrepareResponseHeaders(HttpContext context)
        {
            context.Response.Headers[HttpHeaderName.AccessControlAllowOrigin] = "*";

            if (context.Request.Headers.ConnectionMustBeClosed())
            {
                context.Response.Headers[HttpHeaderName.Connection] = "close";
            }
        }

        private async Task SendResponseAsync(HttpContext context)
        {
            var response = _responseWriter.SerializeResponse(context);

            try
            {
                await _client.OutputStream.WriteAsync(response.AsBuffer());
                await _client.OutputStream.FlushAsync();
            }
            catch (Exception exception)
            {
                _log.Verbose("Error while sending HTTP response back to client. " + exception.Message);
            }
        }

        private async Task UpgradeToWebSocketAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = HttpStatusCode.SwitchingProtocols;
            httpContext.Response.Headers[HttpHeaderName.Connection] = "Upgrade";
            httpContext.Response.Headers[HttpHeaderName.Upgrade] = "websocket";
            httpContext.Response.Headers[HttpHeaderName.SecWebSocketAccept] = GenerateWebSocketAccept(httpContext);

            await SendResponseAsync(httpContext);
            _upgradeToWebSocketSessionCallback(new UpgradedToWebSocketSessionEventArgs(httpContext.Request));
        }

        private string GenerateWebSocketAccept(HttpContext httpContext)
        {
            var webSocketKey = httpContext.Request.Headers[HttpHeaderName.SecWebSocketKey];
            var responseKey = webSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            var responseKeyBuffer = Encoding.ASCII.GetBytes(responseKey).AsBuffer();

            var sha1 = HashAlgorithmProvider.OpenAlgorithm("SHA1");
            var sha1Buffer = sha1.HashData(responseKeyBuffer);

            return Convert.ToBase64String(sha1Buffer.ToArray());
        }

        public void Dispose()
        {
            _httpRequestReader?.Dispose();
            _inputStream?.Dispose();
            _outputStream?.Dispose();
            _client?.Dispose();
        }
    }
}