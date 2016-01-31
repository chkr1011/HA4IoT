using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;

namespace HA4IoT.Networking
{
    public class HttpServer
    {
        private readonly StreamSocketListener _serverSocket = new StreamSocketListener();
        private readonly StatusDescriptionProvider _statusDescriptionProvider = new StatusDescriptionProvider();

        public async Task StartAsync(int port)
        {
            await _serverSocket.BindServiceNameAsync(port.ToString());
            _serverSocket.ConnectionReceived += HandleConnection;
        }

        public event EventHandler<RequestReceivedEventArgs> RequestReceived;

        private void HandleConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Task.Factory.StartNew(() => HandleRequests(args.Socket), TaskCreationOptions.LongRunning);
        }

        private async Task HandleRequests(StreamSocket client)
        {
            using (client)
            {
                await HandleRequest(client);
            }
        }

        private async Task HandleRequest(StreamSocket client)
        {
            HttpContext context = null;
            try
            {
                HttpRequest request = await ReadRequest(client);
                if (request == null)
                {
                    return;
                }

                context = new HttpContext(request, new HttpResponse());

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

                ////if (GetConnectionMustBeClosed(context))
                ////{
                ////    return;
                ////}
            }
            catch (Exception exception)
            {
                if (context != null)
                {
                    context.Response.StatusCode = HttpStatusCode.InternalServerError;
                    context.Response.Body = new JsonBody(ExceptionToJson(exception));
                }
            }
            finally
            {
                if (context != null)
                {
                    await TrySendResponse(client, context);
                }
            }
        }

        private async Task<bool> TrySendResponse(StreamSocket client, HttpContext context)
        {
            try
            {
                context.Response.Headers.Add(HttpHeaderNames.Connection, "close");
                context.Response.Headers.Add(HttpHeaderNames.AccessControlAllowOrigin, "*");

                byte[] content = new byte[0];
                if (context.Response.Body != null)
                {
                    content = context.Response.Body.ToByteArray();
                    context.Response.Headers.Add(HttpHeaderNames.ContentType, context.Response.Body.MimeType);

                    if (context.Request.Headers.GetClientSupportsGzipCompression())
                    {
                        content = Compress(content);
                        context.Response.Headers.Add(HttpHeaderNames.ContentEncoding, "gzip");
                    }
                }

                context.Response.Headers.Add(HttpHeaderNames.ContentLength, content.Length);
                
                using (var dataWriter = new DataWriter(client.OutputStream))
                {
                    dataWriter.WriteString(GetHttpResponseText(context.Response));
                    await dataWriter.StoreAsync();

                    if (content.Length > 0)
                    {
                        dataWriter.WriteBytes(content);
                        await dataWriter.StoreAsync();
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Failed to send HTTP response. " + exception.Message);
                return false;
            }
        }

        private string GetHttpResponseText(HttpResponse response)
        {
            var statusDescription = _statusDescriptionProvider.GetDescription(response.StatusCode);

            var buffer = new StringBuilder();
            buffer.AppendLine("HTTP/1.1 " + (int)response.StatusCode + " " + statusDescription);

            foreach (var header in response.Headers)
            {
                buffer.AppendLine(header.ToString());
            }

            buffer.AppendLine();

            return buffer.ToString();
        }

        private byte[] Compress(byte[] content)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    zipStream.Write(content, 0, content.Length);
                }

                return outputStream.ToArray();
            }
        }

        private async Task<HttpRequest> ReadRequest(StreamSocket client)
        {
            try
            {
                IBuffer buffer = new Buffer(2048);
                await client.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);

                byte[] binaryRequest = buffer.ToArray();
                
                HttpRequest request;
                new HttpRequestParser().TryParse(binaryRequest, out request);

                return request;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Failed to read HTTP request. " + exception.Message);
                return null;
            }
        }

        private JsonObject ExceptionToJson(Exception exception)
        {
            var root = new JsonObject();
            root.SetNamedValue("type", JsonValue.CreateStringValue(exception.GetType().Name));
            root.SetNamedValue("message", JsonValue.CreateStringValue(exception.Message));
            root.SetNamedValue("stackTrace", JsonValue.CreateStringValue(exception.StackTrace));
            root.SetNamedValue("source", JsonValue.CreateStringValue(exception.Source));
            return root;
        }
    }
}