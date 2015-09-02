using System;
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
                        context.Response.Body.Append(ExceptionToJson(exception));
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
            var statusDescription = _statusDescriptionProvider.GetDescription(response.StatusCode);

            using (var dataWriter = new DataWriter(client.OutputStream))
            {
                dataWriter.WriteString("HTTP/1.1 " + (int) response.StatusCode + " " + statusDescription +
                                       Environment.NewLine);

                dataWriter.WriteString("Access-Control-Allow-Origin: *" + Environment.NewLine);
                dataWriter.WriteString("Connection: close" + Environment.NewLine);

                string content;
                if (response.Result != null)
                {
                    if (response.Body.Length > 0)
                    {
                        throw new InvalidOperationException("Could not send body content and result at the same time.");
                    }

                    content = response.Result.Stringify();
                    dataWriter.WriteString("Content-Type: application/json" + Environment.NewLine);
                }
                else
                {
                    content = response.Body.ToString();
                    dataWriter.WriteString("Content-Type: text/html" + Environment.NewLine);
                }

                dataWriter.WriteString("Content-Length: " + content.Length + Environment.NewLine);
                
                dataWriter.WriteString(Environment.NewLine);
                dataWriter.WriteString(content);

                await dataWriter.StoreAsync();
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

        private string ExceptionToJson(Exception exception)
        {
            var root = new JsonObject();
            root.SetNamedValue("type", JsonValue.CreateStringValue(exception.GetType().Name));
            root.SetNamedValue("message", JsonValue.CreateStringValue(exception.Message));
            root.SetNamedValue("stackTrace", JsonValue.CreateStringValue(exception.StackTrace));
            root.SetNamedValue("source", JsonValue.CreateStringValue(exception.Source));
            return root.Stringify();
        }
    }
}