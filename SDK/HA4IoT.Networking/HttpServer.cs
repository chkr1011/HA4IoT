using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;

namespace CK.HomeAutomation.Networking
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
                        await SendResponse(args.Socket, context.Response);
                    }
                }
            }
        }

        private async Task SendResponse(StreamSocket client, HttpResponse response)
        {
            try
            {
                var statusDescription = _statusDescriptionProvider.GetDescription(response.StatusCode);

                var responseText = new StringBuilder();
                responseText.AppendFormat("HTTP/1.1 {0} {1}", (int)response.StatusCode, statusDescription + Environment.NewLine);
                responseText.AppendLine("Access-Control-Allow-Origin: *");
                responseText.AppendLine("Connection: close");

                byte[] content;
                string mimeType;
                if (response.Body != null)
                {
                    content = response.Body.ToByteArray();
                    mimeType = response.Body.MimeType;
                }
                else
                {
                    content = new byte[0];
                    mimeType = string.Empty;
                }

                responseText.AppendLine("Content-Type: " + mimeType);
                responseText.AppendLine("Content-Length: " + content.Length);
                responseText.AppendLine();

                using (var dataWriter = new DataWriter(client.OutputStream))
                {
                    dataWriter.WriteString(responseText.ToString());
                    await dataWriter.StoreAsync();

                    dataWriter.WriteBytes(content);
                    await dataWriter.StoreAsync();
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Failed to send HTTP response. " + exception.Message);
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