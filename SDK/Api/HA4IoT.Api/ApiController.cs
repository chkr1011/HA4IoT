using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Networking;

namespace HA4IoT.Api
{
    public class ApiController : IApiController
    {
        private readonly string _name;
        private readonly List<IApiDispatcherEndpoint> _endpoints = new List<IApiDispatcherEndpoint>();
        private readonly Dictionary<string, Action<IApiContext>> _requestRoutes = new Dictionary<string, Action<IApiContext>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Action<IApiContext>> _commandRoutes = new Dictionary<string, Action<IApiContext>>(StringComparer.OrdinalIgnoreCase);
        private readonly HashAlgorithmProvider _hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

        public ApiController(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _name = name;
        }

        public void NotifyStateChanged(IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));
 
            foreach (var endpoint in _endpoints)
            {
                endpoint.NotifyStateChanged(actuator);
            }
            // TODO: Use information for optimized state generation, pushing to Azure, writing Csv etc.
        }

        public void RouteRequest(string uri, Action<IApiContext> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _requestRoutes.Add(GenerateUri(uri), handler);
        }

        public void RouteCommand(string uri, Action<IApiContext> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _commandRoutes.Add(GenerateUri(uri), handler);
        }
        
        public void RegisterEndpoint(IApiDispatcherEndpoint endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            _endpoints.Add(endpoint);
            endpoint.RequestReceived += RouteRequest;
        }

        private string GenerateUri(string relativePath)
        {
            return $"/{_name}/{relativePath}".Trim();
        }

        private void RouteRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            string uri = e.Context.Uri.Trim();

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
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                handler(apiContext);

                stopwatch.Stop();

                var metaInformation = new JsonObject();

                if (apiContext.CallType == ApiCallType.Request)
                {
                    metaInformation.SetNamedValue("Hash", GenerateHash(apiContext.Response).ToJsonValue());
                }

                metaInformation.SetNamedValue("ProcessingDuration", stopwatch.ElapsedMilliseconds.ToJsonValue());

                apiContext.Response.SetNamedValue("Meta", metaInformation);
            }
            catch (Exception exception)
            {
                apiContext.ResultCode = ApiResultCode.InternalError;
                apiContext.Response = exception.ToJsonObject();
            }
        }

        private string GenerateHash(JsonObject input)
        {
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input.Stringify(), BinaryStringEncoding.Utf8);
            IBuffer hashBuffer = _hashAlgorithm.HashData(buffer);

            return CryptographicBuffer.EncodeToBase64String(hashBuffer);
        }
    }
}
