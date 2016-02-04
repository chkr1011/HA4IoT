using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;

namespace HA4IoT.Networking
{
    public sealed class HttpClientHandler : IDisposable
    {
        private const int REQUEST_BUFFER_SIZE = 2048;

        private readonly StreamSocket _client;
        private readonly DataWriter _dataWriter;
        private readonly HttpRequestParser _requestParser = new HttpRequestParser();
        private readonly HttpResponseSerializer _responseSerializer = new HttpResponseSerializer();

        private bool _abort;

        public HttpClientHandler(StreamSocket client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            _client = client;
            _dataWriter = new DataWriter(_client.OutputStream);
        }

        public event EventHandler<RequestReceivedEventArgs> RequestReceived;

        public async Task HandleRequests()
        {
            while (!_abort)
            {
                await HandleRequest();
            }
        }

        private async Task HandleRequest()
        {
            HttpRequest request = await ReceiveRequest();
            if (request == null)
            {
                _abort = true;
                return;
            }
            
            var context = new HttpContext(request, new HttpResponse());
            PrepareResponseHeaders(context);

            ProcessRequest(context);
            await SendResponse(context);

            if (context.Request.Headers.GetConnectionMustBeClosed())
            {
                _abort = true;
            }
        }

        private async Task<HttpRequest> ReceiveRequest()
        {
            var buffer = new Buffer(REQUEST_BUFFER_SIZE);
            await _client.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);

            byte[] binaryRequest = buffer.ToArray();

            HttpRequest httpRequest;
            _requestParser.TryParse(binaryRequest, out httpRequest);

            return httpRequest;
        }

        private void ProcessRequest(HttpContext context)
        {
            try
            {
                EventHandler<RequestReceivedEventArgs> handler = RequestReceived;
                if (handler == null)
                {
                    context.Response.StatusCode = HttpStatusCode.NotImplemented;
                }
                else
                {
                    var eventArgs = new RequestReceivedEventArgs(context);
                    handler.Invoke(this, eventArgs);

                    if (!eventArgs.IsHandled)
                    {
                        context.Response.StatusCode = HttpStatusCode.BadRequest;
                    }
                }
            }
            catch (Exception exception)
            {
                if (context != null)
                {
                    context.Response.StatusCode = HttpStatusCode.InternalServerError;
                    context.Response.Body = new JsonBody(ExceptionToJson(exception));
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

        private async Task SendResponse(HttpContext context)
        {
            byte[] response = _responseSerializer.SerializeResponse(context);

            _dataWriter.WriteBytes(response);
            await _dataWriter.StoreAsync();
        }

        private JsonObject ExceptionToJson(Exception exception)
        {
            var root = new JsonObject();
            root.SetNamedValue("type", exception.GetType().Name.ToJsonValue());
            root.SetNamedValue("message", exception.Message.ToJsonValue());
            root.SetNamedValue("stackTrace", exception.StackTrace.ToJsonValue());
            root.SetNamedValue("source", exception.Source.ToJsonValue());
            return root;
        }

        public void Dispose()
        {
            _dataWriter.Dispose();
            _client.Dispose();
        }
    }
}
