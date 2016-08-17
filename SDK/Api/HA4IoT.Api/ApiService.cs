using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Api
{
    public class ApiService : IApiService
    {
        private readonly List<IApiDispatcherEndpoint> _endpoints = new List<IApiDispatcherEndpoint>();
        private readonly Dictionary<string, Action<IApiContext>> _requestRoutes = new Dictionary<string, Action<IApiContext>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Action<IApiContext>> _commandRoutes = new Dictionary<string, Action<IApiContext>>(StringComparer.OrdinalIgnoreCase);
        private readonly HashAlgorithmProvider _hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

        public ApiService()
        {
            RouteRequest("requests", HandleRequestApiDescription);
            RouteRequest("status", HandleStatusRequest);
            RouteRequest("configuration", HandleConfigurationRequest);
        }

        public event EventHandler<ApiRequestReceivedEventArgs> StatusRequested;
        public event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequested;

        public void NotifyStateChanged(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
 
            foreach (var endpoint in _endpoints)
            {
                endpoint.NotifyStateChanged(component);
            }
            // TODO: Use information for optimized state generation, pushing to Azure, writing Csv etc.
        }

        public void RouteRequest(string uri, Action<IApiContext> handler)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _requestRoutes.Add(GenerateUri(uri), handler);
        }

        public void RouteCommand(string uri, Action<IApiContext> handler)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _commandRoutes.Add(GenerateUri(uri), handler);
        }

        public void Route(string uri, Action<IApiContext> handler)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _requestRoutes.Add(GenerateUri(uri), handler);
            _commandRoutes.Add(GenerateUri(uri), handler);
        }

        public void Expose(string baseUri, object controller)
        {
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            foreach (var method in controller.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var methodAttribute = method.GetCustomAttribute<ApiMethodAttribute>();
                if (methodAttribute == null)
                {
                    continue;
                }

                string uri = $"{baseUri}/{method.Name}";
                Action<IApiContext> handler = apiContext => method.Invoke(controller, new object[] { apiContext });

                if (methodAttribute.CallType == ApiCallType.Command)
                {
                    RouteCommand(uri, handler);
                }
                else if (methodAttribute.CallType == ApiCallType.Request)
                {
                    RouteRequest(uri, handler);
                }

                Log.Verbose($"Exposed API method to: {uri}");
            }
        }

        public void RegisterEndpoint(IApiDispatcherEndpoint endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            _endpoints.Add(endpoint);
            endpoint.RequestReceived += RouteRequest;
        }

        private void HandleStatusRequest(IApiContext apiContext)
        {
            apiContext.Response.SetNamedString("type", "HA4IoT.Status");
            apiContext.Response.SetNamedNumber("version", 1D);

            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);
            StatusRequested?.Invoke(this, eventArgs);
        }

        private void HandleConfigurationRequest(IApiContext apiContext)
        {
            apiContext.Response.SetNamedString("type", "HA4IoT.Configuration");
            apiContext.Response.SetNamedNumber("version", 1D);

            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);
            ConfigurationRequested?.Invoke(this, eventArgs);
        }

        private string GenerateUri(string relativePath)
        {
            return $"/api/{relativePath}".Trim();
        }

        private void RouteRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            var uri = e.Context.Uri.Trim();

            Action<IApiContext> handler;
            if (e.Context.CallType == ApiCallType.Request && _requestRoutes.TryGetValue(uri, out handler))
            {
                e.IsHandled = true;
                HandleRequest(e.Context, handler);

                return;
            }

            if (e.Context.CallType == ApiCallType.Command && _commandRoutes.TryGetValue(uri, out handler))
            {
                e.IsHandled = true;
                HandleRequest(e.Context, handler);

                return;
            }

            e.Context.ResultCode = ApiResultCode.UnknownUri;
        }

        private void HandleRequest(IApiContext apiContext, Action<IApiContext> handler)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                handler(apiContext);
                stopwatch.Stop();

                var metaInformation = new JsonObject();
                metaInformation.SetNamedNullValue("Hash");
                metaInformation.SetNamedNullValue("ProcessingDuration");
                apiContext.Response.SetNamedObject("Meta", metaInformation);

                string hash = null;
                if (apiContext.CallType == ApiCallType.Request)
                {
                    hash = GenerateHash(apiContext.Response.Stringify());
                }

                metaInformation.SetNamedString("Hash", hash);
                metaInformation.SetNamedNumber("ProcessingDuration", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception exception)
            {
                apiContext.ResultCode = ApiResultCode.InternalError;
                apiContext.Response = ConvertExceptionToJsonObject(exception);
            }
        }

        private string GenerateHash(string input)
        {
            var buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            var hashBuffer = _hashAlgorithm.HashData(buffer);

            return CryptographicBuffer.EncodeToBase64String(hashBuffer);
        }

        private void HandleRequestApiDescription(IApiContext apiContext)
        {
            var requestRoutes = new JsonArray();
            foreach (var requestRoute in _requestRoutes)
            {
                requestRoutes.Add(JsonValue.CreateStringValue(requestRoute.Key));
            }

            apiContext.Response.SetNamedArray("Requests", requestRoutes);

            var commandRoutes = new JsonArray();
            foreach (var commandRoute in _commandRoutes)
            {
                commandRoutes.Add(JsonValue.CreateStringValue(commandRoute.Key));
            }

            apiContext.Response.SetNamedArray("Commands", requestRoutes);
        }

        private JsonObject ConvertExceptionToJsonObject(Exception exception)
        {
            // Do not use a generic serializer because sometines not all propterties are readable
            // and throwing exceptions in the getter.
            var jsonObject = new JsonObject()
                .WithNamedString("Type", exception.GetType().FullName)
                .WithNamedString("Source", exception.Source)
                .WithNamedString("Message", exception.Message)
                .WithNamedString("StackTrace", exception.StackTrace);

            if (exception.InnerException != null)
            {
                jsonObject.SetNamedValue("InnerException", ConvertExceptionToJsonObject(exception.InnerException));
            }
            else
            {
                jsonObject.SetNamedValue("InnerException", JsonValue.CreateNullValue());
            }

            return jsonObject;
        }
    }
}
