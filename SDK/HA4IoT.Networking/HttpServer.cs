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

        private async void HandleConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            using (args.Socket)
            {
                HttpContext context = null;
                try
                {
                    HttpRequest request = await ReadRequest(args.Socket);
                    if (request != null)
                    {
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
                finally
                {
                    if (context != null)
                    {
                        await SendResponse(args.Socket, context);
                    }
                }
            }
        }

        private async Task SendResponse(StreamSocket client, HttpContext context)
        {
            try
            {
                var statusDescription = _statusDescriptionProvider.GetDescription(context.Response.StatusCode);

                var responseText = new StringBuilder();
                responseText.AppendLine("HTTP/1.1 " + (int)context.Response.StatusCode + " " + statusDescription);
                //responseText.AppendLine("Access-Control-Allow-Origin:*");
                responseText.AppendLine("Connection:close");

                byte[] content;
                if (context.Response.Body != null)
                {
                    content = context.Response.Body.ToByteArray();
                    context.Response.Headers.Add("Content-Type", context.Response.Body.MimeType);

                    if (GetClientSupportsCompression(context.Request))
                    {
                        content = Compress(content);
                        context.Response.Headers.Add("Content-Encoding", "gzip");
                    }
                }
                else
                {
                    content = new byte[0];
                }

                context.Response.Headers.Add("Content-Length", content.Length);

                foreach (var header in context.Response.Headers)
                {
                    responseText.AppendLine(header.ToString());
                }

                responseText.AppendLine();
                
                using (var dataWriter = new DataWriter(client.OutputStream))
                {
                    dataWriter.WriteString(responseText.ToString());
                    await dataWriter.StoreAsync();

                    if (content.Length > 0)
                    {
                        dataWriter.WriteBytes(content);
                        await dataWriter.StoreAsync();
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Failed to send HTTP response. " + exception.Message);
            }
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
            IBuffer buffer = new Buffer(2048);
            await client.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);

            var binaryRequest = buffer.ToArray();
            var requestText = Encoding.ASCII.GetString(binaryRequest, 0, binaryRequest.Length);

            HttpRequest request;
            new HttpRequestParser(requestText).TryParse(out request);

            return request;
        }

        private bool GetClientSupportsCompression(HttpRequest request)
        {
            string headerValue;
            if (request.Headers.TryGetValue("Accept-Encoding", out headerValue))
            {
                return headerValue.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) > -1;
            }

            return false;
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