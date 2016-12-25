using System;
using System.Collections.Generic;
using System.Reflection;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Networking.Json;

namespace HA4IoT.Api
{
    public class ApiDispatcherService : ServiceBase, IApiDispatcherService
    {
        private readonly List<IApiAdapter> _adapters = new List<IApiAdapter>();
        private readonly Dictionary<string, Action<IApiContext>> _routes = new Dictionary<string, Action<IApiContext>>(StringComparer.OrdinalIgnoreCase);
        
        public ApiDispatcherService()
        {
            Route("Status", HandleStatusRequest);
            Route("Configuration", HandleConfigurationRequest);
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
            // TODO: Use information for optimized state generation, pushing to Azure, writing Csv etc.
        }

        public void Route(string action, Action<IApiContext> handler)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _routes.Add(action.Trim(), handler);
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

        public void Expose(string @namespace, object controller)
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

                var action = $"{@namespace}/{method.Name}";
                Action<IApiContext> handler = apiContext => method.Invoke(controller, new object[] { apiContext });
                Route(action, handler);

                Log.Verbose($"Exposed API method to action '{action}'.");
            }
        }

        public void RegisterAdapter(IApiAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            _adapters.Add(adapter);
            adapter.RequestReceived += RouteRequest;
        }

        private void HandleStatusRequest(IApiContext apiContext)
        {
            apiContext.UseHash = true;

            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);
            StatusRequested?.Invoke(this, eventArgs);
            StatusRequestCompleted?.Invoke(this, eventArgs);
        }

        private void HandleConfigurationRequest(IApiContext apiContext)
        {
            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);
            ConfigurationRequested?.Invoke(this, eventArgs);
            ConfigurationRequestCompleted?.Invoke(this, eventArgs);
        }

        private void RouteRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            Action<IApiContext> handler;
            if (_routes.TryGetValue(e.Context.Action, out handler))
            {
                e.IsHandled = true;
                HandleRequest(e.Context, handler);

                return;
            }

            e.Context.ResultCode = ApiResultCode.NotSupported;
        }

        private static void HandleRequest(IApiContext apiContext, Action<IApiContext> handler)
        {
            try
            {
                handler(apiContext);
            }
            catch (Exception exception)
            {
                apiContext.ResultCode = ApiResultCode.InternalError;
                apiContext.Response = JsonSerializer.SerializeException(exception);
            }
        }
    }
}
