using System;
using System.IO;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking
{
    public sealed class HttpClientHandler : IDisposable
    {
        private const int REQUEST_BUFFER_SIZE = 16*1024;
        private readonly byte[] _buffer = new byte[REQUEST_BUFFER_SIZE];

        private readonly HttpRequestParser _requestParser = new HttpRequestParser();
        private readonly HttpResponseSerializer _responseSerializer = new HttpResponseSerializer();

        private readonly StreamSocket _client;
        private readonly Stream _inputStream;
        private readonly Stream _outputStream;

        private readonly Func<HttpClientHandler, HttpContext, bool> _requestReceivedCallback;

        private bool _abort;

        public HttpClientHandler(StreamSocket client, Func<HttpClientHandler, HttpContext, bool> requestReceivedCallback)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (requestReceivedCallback == null) throw new ArgumentNullException(nameof(requestReceivedCallback));

            _client = client;
            _inputStream = _client.InputStream.AsStreamForRead(_buffer.Length);
            _outputStream = _client.OutputStream.AsStreamForWrite(REQUEST_BUFFER_SIZE);

            _requestReceivedCallback = requestReceivedCallback;
        }

        public void HandleRequests()
        {
            while (!_abort)
            {
                HandleRequest();
            }
        }

        private void HandleRequest()
        {
            HttpRequest request = ReceiveRequest();
            if (request == null)
            {
                _abort = true;
                return;
            }
            
            var context = new HttpContext(request, new HttpResponse());
            PrepareResponseHeaders(context);

            ProcessRequest(context);
            SendResponse(context);

            if (context.Request.Headers.GetConnectionMustBeClosed())
            {
                _abort = true;
            }
        }

        private HttpRequest ReceiveRequest()
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

        private void ProcessRequest(HttpContext context)
        {
            try
            {
                bool requestHandled = _requestReceivedCallback.Invoke(this, context);
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

        public void Dispose()
        {
            _inputStream.Dispose();
            _outputStream.Dispose();
            _client.Dispose();
        }
    }
}
