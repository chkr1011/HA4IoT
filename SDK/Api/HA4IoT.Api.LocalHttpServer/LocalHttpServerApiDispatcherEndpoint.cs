using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;
using HttpMethod = HA4IoT.Contracts.Networking.HttpMethod;

namespace HA4IoT.Api.LocalHttpServer
{
    public class LocalHttpServerApiDispatcherEndpoint : IApiDispatcherEndpoint
    {
        public LocalHttpServerApiDispatcherEndpoint(HttpServer httpRequestController)
        {
            if (httpRequestController == null) throw new ArgumentNullException(nameof(httpRequestController));

            httpRequestController.RequestReceived += DispatchRequest;
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public void NotifyStateChanged(IActuator actuator)
        {
            // Let the NEXT client create a new state which is cached for all.
        }

        private void DispatchRequest(object sender, HttpRequestReceivedEventArgs eventArgs)
        {
            if (!eventArgs.Context.Request.Uri.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            eventArgs.IsHandled = true;
            DispatchRequest(eventArgs.Context);
        }

        private void DispatchRequest(HttpContext httpContext)
        {
            var apiContext = CreateContext(httpContext);
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
                var serverHash = apiContext.Response.GetNamedObject("Meta").GetNamedString("Hash");
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
            
            httpContext.Response.Body = new StringBody(eventArgs.Context.SerializedResponse).WithMimeType(MimeTypeProvider.Json);
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

        private ApiContext CreateContext(HttpContext httpContext)
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
