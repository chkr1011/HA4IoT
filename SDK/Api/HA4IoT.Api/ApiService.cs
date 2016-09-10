using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Networking.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api
{
    public class ApiService : ServiceBase, IApiService
    {
        private readonly List<IApiDispatcherEndpoint> _endpoints = new List<IApiDispatcherEndpoint>();
        private readonly Dictionary<string, Action<IApiContext>> _routes = new Dictionary<string, Action<IApiContext>>(StringComparer.OrdinalIgnoreCase);
        private readonly HashAlgorithmProvider _hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

        public ApiService()
        {
            Route("Status", HandleStatusRequest);
            Route("Configuration", HandleConfigurationRequest);
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

        public void Route(string uri, Action<IApiContext> handler)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _routes.Add(GenerateUri(uri), handler);
        }

        public void Expose(object controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            var controllerType = controller.GetType();

            var classAttribute = controllerType.GetTypeInfo().GetCustomAttribute<ApiClassAttribute>();
            if (classAttribute == null)
            {
                return;
            }

            Expose(classAttribute.Uri, controller);
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
                Route(uri, handler);

                Log.Verbose($"Exposed API method to URI '{uri}'");
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
            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);
            StatusRequested?.Invoke(this, eventArgs);
        }

        private void HandleConfigurationRequest(IApiContext apiContext)
        {
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
            if (_routes.TryGetValue(uri, out handler))
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
                handler(apiContext);

                var metaInformation = new JObject
                {
                    ["Hash"] = null
                };

                apiContext.Response["Meta"] = metaInformation;
                var hash = GenerateHash(apiContext.Response.ToString());
                metaInformation["Hash"] = hash;
            }
            catch (Exception exception)
            {
                apiContext.ResultCode = ApiResultCode.InternalError;
                apiContext.Response = JsonSerializer.SerializeException(exception);
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
