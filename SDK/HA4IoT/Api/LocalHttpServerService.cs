using System;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Api.Cloud;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking.Http;
using HA4IoT.Contracts.Networking.WebSockets;
using HA4IoT.Contracts.Services;
using HA4IoT.Networking.Http;
using HA4IoT.Networking.Json;
using HA4IoT.Networking.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api
{
    public class LocalHttpServerService : ServiceBase, IApiAdapter
    {
        private readonly HashAlgorithmProvider _hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
        private readonly IApiDispatcherService _apiDispatcherService;
        private readonly HttpServer _httpServer;

        public LocalHttpServerService(IApiDispatcherService apiDispatcherService, HttpServer httpServer)
        {
            if (apiDispatcherService == null) throw new ArgumentNullException(nameof(apiDispatcherService));
            if (httpServer == null) throw new ArgumentNullException(nameof(httpServer));

            _apiDispatcherService = apiDispatcherService;
            _httpServer = httpServer;
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public override void Startup()
        {
            _httpServer.RequestReceived += DispatchHttpRequest;
            _httpServer.WebSocketConnected += AttachWebSocket;

            _apiDispatcherService.RegisterAdapter(this);
        }

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
            if (httpContext.Request.Uri.StartsWith("/api/Invoke"))
            {
                DispatchGenericHttpRequest(httpContext);
                return;
            }
            
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

            if (eventArgs.Context.Result == null)
            {
                eventArgs.Context.Result = new JObject();
            }

            httpContext.Response.StatusCode = ConvertResultCode(eventArgs.Context.ResultCode);

            if (apiContext.UseHash)
            {
                var serverHash = GenerateHash(apiContext.Result.ToString());
                eventArgs.Context.Result["$Hash"] = serverHash;

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

            httpContext.Response.Body = new JsonBody(eventArgs.Context.Result);
        }

        private void DispatchGenericHttpRequest(HttpContext httpContext)
        {
            var timestamp = DateTime.UtcNow;

            var genericApiRequest = JsonConvert.DeserializeObject<ApiRequest>(httpContext.Request.Body);
            var apiContext = new ApiContext(genericApiRequest.Action, genericApiRequest.Parameter);

            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);
            RequestReceived?.Invoke(this, eventArgs);
            if (!eventArgs.IsHandled)
            {
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            if (eventArgs.Context.Result == null)
            {
                eventArgs.Context.Result = new JObject();
            }

            httpContext.Response.StatusCode = HttpStatusCode.OK;

            var response = new ApiResponse
            {
                ResultCode = apiContext.ResultCode,
                Result = apiContext.Result,
                InternalProcessingDuration = (int)(DateTime.UtcNow - timestamp).TotalMilliseconds
            };

            httpContext.Response.Body = new JsonBody(JObject.FromObject(response));
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

            var correlationId = (string)requestMessage["CorrelationId"];
            var uri = (string)requestMessage["Uri"];
            var request = (JObject)requestMessage["Content"] ?? new JObject();

            var context = new ApiContext(uri, request);
            var eventArgs = new ApiRequestReceivedEventArgs(context);
            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                context.ResultCode = ApiResultCode.ActionNotSupported;
            }

            var responseMessage = new JObject
            {
                ["CorrelationId"] = correlationId,
                ["ResultCode"] = context.ResultCode.ToString(),
                ["Content"] = context.Result
            };
            
            e.WebSocketClientSession.SendAsync(responseMessage.ToString()).Wait();
        }

        private HttpStatusCode ConvertResultCode(ApiResultCode resultCode)
        {
            switch (resultCode)
            {
                case ApiResultCode.Success: return HttpStatusCode.OK;
                case ApiResultCode.UnhandledException: return HttpStatusCode.InternalServerError;
                case ApiResultCode.ActionNotSupported: return HttpStatusCode.NotFound;
                case ApiResultCode.InvalidParameter: return HttpStatusCode.BadRequest;
            }

            throw new NotSupportedException();
        }

        private ApiContext CreateApiContext(HttpContext httpContext)
        {
            try
            {
                var body = string.IsNullOrEmpty(httpContext.Request.Body) ? new JObject() : JObject.Parse(httpContext.Request.Body);
                var action = httpContext.Request.Uri.Substring("/api/".Length);

                return new ApiContext(action, body);
            }
            catch (Exception)
            {
                Log.Verbose("Received a request with no valid JSON request.");

                return null;
            }
        }

        private string GenerateHash(string input)
        {
            var buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            var hashBuffer = _hashAlgorithm.HashData(buffer);

            return CryptographicBuffer.EncodeToBase64String(hashBuffer);
        }
    }
}
