using System;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking.Http;
using HA4IoT.Contracts.Networking.WebSockets;
using HA4IoT.Contracts.Services;
using HA4IoT.Networking.Http;
using HA4IoT.Networking.Json;
using HA4IoT.Networking.WebSockets;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api
{
    public class LocalHttpServerApiDispatcherEndpointService : ServiceBase, IApiDispatcherEndpoint
    {
        private readonly HashAlgorithmProvider _hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
        private readonly IApiService _apiService;
        private readonly HttpServer _httpServer;

        public LocalHttpServerApiDispatcherEndpointService(IApiService apiService, HttpServer httpServer)
        {
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (httpServer == null) throw new ArgumentNullException(nameof(httpServer));

            _apiService = apiService;
            _httpServer = httpServer;
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public override void Startup()
        {
            _httpServer.RequestReceived += DispatchHttpRequest;
            _httpServer.WebSocketConnected += AttachWebSocket;

            _apiService.RegisterEndpoint(this);
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

            if (apiContext.UseHash)
            {
                var serverHash = GenerateHash(apiContext.Response.ToString());
                eventArgs.Context.Response["$Hash"] = serverHash;

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
            var requestMessage = JObject.Parse(((WebSocketTextMessage)e.Message).Text);

            var correlationId = (string)requestMessage["CorrelationId"];
            var uri = (string)requestMessage["Uri"];
            var request = (JObject)requestMessage["Content"] ?? new JObject();

            var context = new ApiContext(uri, request, new JObject());
            var eventArgs = new ApiRequestReceivedEventArgs(context);
            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                context.ResultCode = ApiResultCode.UnknownUri;
            }

            var responseMessage = new JObject
            {
                ["CorrelationId"] = correlationId,
                ["ResultCode"] = context.ResultCode.ToString(),
                ["Content"] = context.Response
            };
            
            e.WebSocketClientSession.SendAsync(responseMessage.ToString()).Wait();
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
                var request = string.IsNullOrEmpty(httpContext.Request.Body) ? new JObject() : JObject.Parse(httpContext.Request.Body);
                return new ApiContext(httpContext.Request.Uri, request, new JObject());
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
