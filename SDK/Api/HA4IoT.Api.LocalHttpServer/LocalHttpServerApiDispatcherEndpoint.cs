using System;
using System.Diagnostics;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking.Http;
using HA4IoT.Contracts.Networking.WebSockets;
using HA4IoT.Networking.Http;
using HA4IoT.Networking.Json;
using HA4IoT.Networking.WebSockets;
using HttpMethod = HA4IoT.Contracts.Networking.Http.HttpMethod;

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
                eventArgs.Context.Response = new JsonObject();
            }

            httpContext.Response.StatusCode = ConvertResultCode(eventArgs.Context.ResultCode);

            if (apiContext.CallType == ApiCallType.Request)
            {
                var serverHash = apiContext.Response.GetNamedObject("Meta", new JsonObject()).GetNamedString("Hash", string.Empty);
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
            }

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

                var requestMessage = JsonObject.Parse(((WebSocketTextMessage)e.Message).Text);

                var correlationId = requestMessage.GetNamedString("CorrelationId", string.Empty);

                var uri = requestMessage.GetNamedString("Uri", string.Empty);
                if (string.IsNullOrEmpty(uri))
                {
                    Log.Warning("Received WebSocket message with missing or invalid URI property.");
                    return;
                }

                var callTypeSource = requestMessage.GetNamedString("CallType", string.Empty);

                ApiCallType callType;
                if (!Enum.TryParse(callTypeSource, true, out callType))
                {
                    Log.Warning("Received WebSocket message with missing or invalid CallType property.");
                    return;
                }

                var request = requestMessage.GetNamedObject("Content", new JsonObject());

                var context = new ApiContext(callType, uri, request, new JsonObject());
                var eventArgs = new ApiRequestReceivedEventArgs(context);
                RequestReceived?.Invoke(this, eventArgs);

                if (!eventArgs.IsHandled)
                {
                    Log.Warning("WebSocket message is not handled.");
                    return;
                }

                processingStopwatch.Stop();

                //var correlationId = context.BrokerProperties.GetNamedString("CorrelationId", string.Empty);
                var clientEtag = context.Request.GetNamedString("ETag", string.Empty);

                var responseMessage = new JsonObject();
                responseMessage.SetValue("CorrelationId", correlationId);
                responseMessage.SetValue("ResultCode", context.ResultCode);
                responseMessage.SetValue("ProcessingDuration", processingStopwatch.ElapsedMilliseconds);

                if (context.CallType == ApiCallType.Request)
                {
                    string serverEtag = context.Response.GetNamedObject("Meta", new JsonObject()).GetNamedString("Hash", string.Empty);
                    responseMessage.SetValue("ETag", serverEtag);

                    if (!string.Equals(clientEtag, serverEtag))
                    {
                        responseMessage.SetNamedValue("Content", context.Response);
                    }
                }
                else
                {
                    responseMessage.SetNamedValue("Content", context.Response);
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
            ApiCallType callType;
            if (httpContext.Request.Method == HttpMethod.Get)
            {
                callType = ApiCallType.Request;
            }
            else if (httpContext.Request.Method == HttpMethod.Post)
            {
                callType = ApiCallType.Command;
            }
            else
            {
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return null;
            }

            JsonObject body = ParseJson(httpContext.Request.Body);
            return new ApiContext(callType, httpContext.Request.Uri, body, new JsonObject());
        }

        private JsonObject ParseJson(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new JsonObject();
            }

            return JsonObject.Parse(source);
        }
    }
}
