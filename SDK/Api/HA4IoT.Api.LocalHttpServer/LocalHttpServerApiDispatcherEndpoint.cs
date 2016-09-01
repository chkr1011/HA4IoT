using System;
using System.Diagnostics;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking.Http;
using HA4IoT.Contracts.Networking.WebSockets;
using HA4IoT.Networking.Http;
using HA4IoT.Networking.Json;
using HA4IoT.Networking.WebSockets;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api.LocalHttpServer
{
    public class LocalHttpServerApiDispatcherEndpoint : IApiDispatcherEndpoint
    {
        public LocalHttpServerApiDispatcherEndpoint(HttpServer httpServer)
        {
            if (httpServer == null) throw new ArgumentNullException(nameof(httpServer));

            httpServer.RequestReceived += DispatchHttpRequest;
            httpServer.WebSocketConnected += AttachWebSocket;
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public void NotifyStateChanged(IComponent component)
        {
            // Let the NEXT client create a new state which is cached for all.
        }

        private void DispatchHttpRequest(object sender, HttpRequestReceivedEventArgs eventArgs)
        {
            if (!eventArgs.Context.Request.Uri.StartsWith("/api/"))
            {
                return;
            }

            eventArgs.IsHandled = true;
            DispatchHttpRequest(eventArgs.Context);
        }

        private void DispatchHttpRequest(HttpContext httpContext)
        {
            var apiContext = CreateApiContext(httpContext);
            if (apiContext == null)
            {
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);
            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            if (eventArgs.Context.Response == null)
            {
                eventArgs.Context.Response = new JObject();
            }

            httpContext.Response.StatusCode = ConvertResultCode(eventArgs.Context.ResultCode);

            var serverHash = (string)apiContext.Response["Meta"]["Hash"];
            var serverHashWithQuotes = "\"" + serverHash + "\"";

            string clientHash;
            if (httpContext.Request.Headers.TryGetValue(HttpHeaderNames.IfNoneMatch, out clientHash))
            {
                if (clientHash.Equals(serverHashWithQuotes))
                {
                    httpContext.Response.StatusCode = HttpStatusCode.NotModified;
                    return;
                }
            }

            httpContext.Response.Headers[HttpHeaderNames.ETag] = serverHashWithQuotes;
            httpContext.Response.Body = new JsonBody(eventArgs.Context.Response);
        }

        private void AttachWebSocket(object sender, WebSocketConnectedEventArgs eventArgs)
        {
            // Accept each URI at the moment.
            eventArgs.IsHandled = true;

            AttachWebSocket(eventArgs.WebSocketClientSession);
        }

        private void AttachWebSocket(IWebSocketClientSession webSocketClientSession)
        {
            webSocketClientSession.MessageReceived += DispatchWebSocketMessage;
            webSocketClientSession.Closed += (s, e) => webSocketClientSession.MessageReceived -= DispatchWebSocketMessage;
        }

        private void DispatchWebSocketMessage(object sender, WebSocketMessageReceivedEventArgs e)
        {
            try
            {
                Stopwatch processingStopwatch = Stopwatch.StartNew();

                // TODO: Create separate class for this format which can be used for azure too.

                var requestMessage = JObject.Parse(((WebSocketTextMessage)e.Message).Text);

                var correlationId = (string)requestMessage["CorrelationId"];

                var uri = (string)requestMessage["Uri"];
                if (string.IsNullOrEmpty(uri))
                {
                    Log.Warning("Received WebSocket message with missing or invalid URI property.");
                    return;
                }

                var callTypeSource = (string)requestMessage["CallType"];

                var request = (JObject)requestMessage["Content"];

                var context = new ApiContext(uri, request, new JObject());
                var eventArgs = new ApiRequestReceivedEventArgs(context);
                RequestReceived?.Invoke(this, eventArgs);

                if (!eventArgs.IsHandled)
                {
                    Log.Warning("WebSocket message is not handled.");
                    return;
                }

                processingStopwatch.Stop();

                //var correlationId = context.BrokerProperties.GetNamedString("CorrelationId", string.Empty);
                var clientEtag = (string) context.Request["ETag"];

                var responseMessage = new JObject
                {
                    ["CorrelationId"] = correlationId,
                    ["ResultCode"] = context.ResultCode.ToString(),
                    ["ProcessingDuration"] = processingStopwatch.ElapsedMilliseconds
                };

                var serverEtag = (string)context.Response["Meta"]["Hash"];
                responseMessage["ETag"] = serverEtag;

                if (!string.Equals(clientEtag, serverEtag))
                {
                    responseMessage["Content"] = context.Response;
                }

                e.WebSocketClientSession.SendAsync(responseMessage.ToString()).Wait();
            }
            catch (Exception)
            {
                throw;
            }

        }

        private HttpStatusCode ConvertResultCode(ApiResultCode resultCode)
        {
            switch (resultCode)
            {
                case ApiResultCode.Success: return HttpStatusCode.OK;
                case ApiResultCode.InternalError: return HttpStatusCode.InternalServerError;
                case ApiResultCode.UnknownUri: return HttpStatusCode.NotFound;
                case ApiResultCode.InvalidBody: return HttpStatusCode.BadRequest;
            }

            throw new NotSupportedException();
        }

        private ApiContext CreateApiContext(HttpContext httpContext)
        {
            try
            {
                JObject request;
                if (string.IsNullOrEmpty(httpContext.Request.Body))
                {
                    request = new JObject();
                }
                else
                {
                    request = JObject.Parse(httpContext.Request.Body);
                }
                
                return new ApiContext(httpContext.Request.Uri, request, new JObject());
            }
            catch (Exception)
            {
                Log.Verbose("Received a request with no valid JSON request.");

                return null;
            }
        }
    }
}
