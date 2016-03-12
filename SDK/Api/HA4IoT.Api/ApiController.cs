using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Networking;

namespace HA4IoT.Api
{
    public class ApiController : IApiController
    {
        private readonly string _name;
        private readonly Dictionary<string, Action<IApiContext>> _requestRoutes = new Dictionary<string, Action<IApiContext>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Action<IApiContext>> _commandRoutes = new Dictionary<string, Action<IApiContext>>(StringComparer.OrdinalIgnoreCase);

        public ApiController(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _name = name;
        }

        public void NotifyStateChanged(IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

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
            try
            {
                handler(apiContext);
            }
            catch (Exception exception)
            {
                apiContext.ResultCode = ApiResultCode.InternalError;
                apiContext.Response = exception.ToJsonObject();
            }
        }
    }
}
