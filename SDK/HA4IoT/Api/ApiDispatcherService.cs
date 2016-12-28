using System;
using System.Collections.Generic;
using System.Reflection;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Networking.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api
{
    public class ApiDispatcherService : ServiceBase, IApiDispatcherService
    {
        private readonly List<IApiAdapter> _adapters = new List<IApiAdapter>();
        private readonly Dictionary<string, Action<IApiContext>> _routes = new Dictionary<string, Action<IApiContext>>(StringComparer.OrdinalIgnoreCase);
        
        public ApiDispatcherService()
        {
            Route("Status", HandleGetStatusRequest);
            Route("Configuration", HandleGetConfigurationRequest);
            Route("Routes", HandleGetRoutesRequest);
        }

        public event EventHandler<ApiRequestReceivedEventArgs> StatusRequested;
        public event EventHandler<ApiRequestReceivedEventArgs> StatusRequestCompleted;
        public event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequested;
        public event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequestCompleted;

        public void NotifyStateChanged(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
 
            foreach (var adapter in _adapters)
            {
                adapter.NotifyStateChanged(component);
            }
        }

        public void Route(string action, Action<IApiContext> handler)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_routes)
            {
                _routes.Add(action.Trim(), handler);
            }
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

            Expose(classAttribute.Namespace, controller);
        }

        public void RegisterAdapter(IApiAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            _adapters.Add(adapter);
            adapter.RequestReceived += RouteRequest;
        }

        private void Expose(string @namespace, object controller)
        {
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            foreach (var method in controller.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var methodAttribute = method.GetCustomAttribute<ApiMethodAttribute>();
                if (methodAttribute == null)
                {
                    continue;
                }

                var action = @namespace + "/" + method.Name;
                Action<IApiContext> handler = apiContext => method.Invoke(controller, new object[] { apiContext });
                Route(action, handler);

                Log.Verbose($"Exposed API method to action '{action}'.");
            }
        }

        private void HandleGetRoutesRequest(IApiContext apiContext)
        {
            var routes = new JArray();

            lock (_routes)
            {
                foreach (var route in _routes)
                {
                    routes.Add(route.Key);
                }
            }

            apiContext.Response.Add("Routes", routes);
        }

        private void HandleGetStatusRequest(IApiContext apiContext)
        {
            apiContext.UseHash = true;

            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);
            StatusRequested?.Invoke(this, eventArgs);
            StatusRequestCompleted?.Invoke(this, eventArgs);
        }

        private void HandleGetConfigurationRequest(IApiContext apiContext)
        {
            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);
            ConfigurationRequested?.Invoke(this, eventArgs);
            ConfigurationRequestCompleted?.Invoke(this, eventArgs);
        }

        private void RouteRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            Action<IApiContext> action;
            lock (_routes)
            {
                if (!_routes.TryGetValue(e.Context.Action, out action))
                {
                    e.Context.ResultCode = ApiResultCode.ActionNotSupported;
                    return;
                }
            }

            e.IsHandled = true;
            TryHandleRequest(e.Context, action);
        }

        private static void TryHandleRequest(IApiContext apiContext, Action<IApiContext> action)
        {
            try
            {
                action(apiContext);
            }
            catch (Exception exception)
            {
                apiContext.ResultCode = ApiResultCode.UnhandledException;
                apiContext.Response = JsonSerializer.SerializeException(exception);
            }
        }
    }
}
