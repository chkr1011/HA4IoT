using System;
using System.Net;
using System.Text;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Networking.Http;
using HA4IoT.Networking.Json;
using HA4IoT.Networking.WebSockets;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api
{
    public class LocalHttpServerService : ServiceBase, IApiAdapter
    {
        private readonly IApiDispatcherService _apiDispatcherService;
        private readonly HttpServer _httpServer;
        private readonly ILogger _log;

        public LocalHttpServerService(IApiDispatcherService apiDispatcherService, HttpServer httpServer, ILogService logService)
        {
            _apiDispatcherService = apiDispatcherService ?? throw new ArgumentNullException(nameof(apiDispatcherService));
            _httpServer = httpServer ?? throw new ArgumentNullException(nameof(httpServer));

            _log = logService?.CreatePublisher(nameof(LocalHttpServerService)) ?? throw new ArgumentNullException(nameof(logService));
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public override void Startup()
        {
            _httpServer.HttpRequestReceived += DispatchHttpRequest;
            _httpServer.WebSocketConnected += AttachWebSocket;

            _apiDispatcherService.RegisterAdapter(this);
        }

        public void NotifyStateChanged(IComponent component)
        {
        }

        private void DispatchHttpRequest(object sender, HttpRequestReceivedEventArgs eventArgs)
        {
            if (!eventArgs.Context.Request.Uri.StartsWith("/api/"))
            {
                return;
            }

            DispatchHttpRequest(eventArgs.Context);
            eventArgs.IsHandled = true;
        }

        private void DispatchHttpRequest(HttpContext httpContext)
        {
            IApiContext apiContext = CreateApiContext(httpContext);
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

            httpContext.Response.StatusCode = HttpStatusCode.OK;
            if (eventArgs.ApiContext.Result == null)
            {
                eventArgs.ApiContext.Result = new JObject();
            }

            var apiResponse = new ApiResponse
            {
                ResultCode = apiContext.ResultCode,
                Result = apiContext.Result,
                ResultHash = apiContext.ResultHash
            };

            httpContext.Response.Body = new JsonBody(JObject.FromObject(apiResponse));
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
            var requestMessage = JObject.Parse(((WebSocketTextMessage)e.Message).Text);
            var apiRequest = requestMessage.ToObject<ApiRequest>();

            var context = new ApiContext(apiRequest.Action, apiRequest.Parameter, apiRequest.ResultHash);
            var eventArgs = new ApiRequestReceivedEventArgs(context);
            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                context.ResultCode = ApiResultCode.ActionNotSupported;
            }

            var responseMessage = new JObject
            {
                ["CorrelationId"] = requestMessage["CorrelationId"].Value<string>(),
                ["ResultCode"] = context.ResultCode.ToString(),
                ["Content"] = context.Result
            };

            e.WebSocketClientSession.SendAsync(responseMessage.ToString()).Wait();
        }

        private ApiContext CreateApiContext(HttpContext httpContext)
        {
            try
            {
                string bodyText;

                // Parse a special query parameter.
                if (!string.IsNullOrEmpty(httpContext.Request.Query) && httpContext.Request.Query.StartsWith("body=", StringComparison.OrdinalIgnoreCase))
                {
                    bodyText = Uri.UnescapeDataString(httpContext.Request.Query.Substring("body=".Length));
                }
                else
                {
                    bodyText = Encoding.UTF8.GetString(httpContext.Request.Body ?? new byte[0]);
                }

                var action = httpContext.Request.Uri.Substring("/api/".Length);
                var parameter = string.IsNullOrEmpty(bodyText) ? new JObject() : JObject.Parse(bodyText);

                return new ApiContext(action, parameter, null);
            }
            catch (Exception)
            {
                _log.Verbose("Received a request with no valid JSON request.");
                return null;
            }
        }
    }
}
