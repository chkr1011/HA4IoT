using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api
{
    public class ApiDispatcherService : ServiceBase, IApiDispatcherService
    {
        private static readonly HashAlgorithmProvider HashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

        private readonly List<IApiAdapter> _adapters = new List<IApiAdapter>();
        private readonly Dictionary<string, Action<IApiContext>> _actions = new Dictionary<string, Action<IApiContext>>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger _log;

        public ApiDispatcherService(ILogService logService)
        {
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            Route("GetStatus", HandleGetStatusRequest);
            Route("GetConfiguration", HandleGetConfigurationRequest);
            Route("GetActions", HandleGetActionsRequest);
            Route("Ping", HandlePingRequest);
            Route("Execute", HandleExecuteRequest);

            _log = logService.CreatePublisher(nameof(ApiDispatcherService));
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

            lock (_actions)
            {
                action = action.Trim();

                if (_actions.ContainsKey(action))
                {
                    _log.Warning($"Overriding action route: {action}");    
                }

                _actions[action] = handler;
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
                void Handler(IApiContext apiContext) => method.Invoke(controller, new object[] {apiContext});
                Route(action, Handler);

                _log.Verbose($"Exposed API method to action '{action}'.");
            }
        }

        private void HandleGetActionsRequest(IApiContext apiContext)
        {
            var actions = new JArray();

            lock (_actions)
            {
                foreach (var action in _actions)
                {
                    actions.Add(action.Key);
                }
            }

            apiContext.Result.Add("Actions", actions);
        }

        private void HandleGetStatusRequest(IApiContext apiContext)
        {
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

        private void HandlePingRequest(IApiContext apiContext)
        {
            apiContext.ResultCode = ApiResultCode.Success;
            apiContext.Result = apiContext.Parameter;
        }

        private void HandleExecuteRequest(IApiContext apiContext)
        {
            if (apiContext.Parameter == null || string.IsNullOrEmpty(apiContext.Action))
            {
                apiContext.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }
            
            var apiRequest = apiContext.Parameter.ToObject<ApiRequest>();
            if (apiRequest == null)
            {
                apiContext.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            if (apiRequest.Action.Equals("Execute", StringComparison.OrdinalIgnoreCase))
            {
                apiContext.ResultCode = ApiResultCode.ActionNotSupported;
                return;
            }

            var innerApiContext = new ApiContext(apiRequest.Action, apiRequest.Parameter ?? new JObject(), apiRequest.ResultHash);

            var eventArgs = new ApiRequestReceivedEventArgs(innerApiContext);
            RouteRequest(this, eventArgs);

            apiContext.ResultCode = innerApiContext.ResultCode;
            apiContext.Result = innerApiContext.Result;
            apiContext.ResultHash = innerApiContext.ResultHash;

            if (apiContext.ResultHash != null)
            {
                var newHash = GenerateHash(apiContext.Result.ToString());
                if (apiContext.ResultHash.Equals(newHash))
                {
                    apiContext.Result = new JObject();
                }

                apiContext.ResultHash = newHash;
            }
        }

        private void RouteRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            Action<IApiContext> handler;
            lock (_actions)
            {
                if (!_actions.TryGetValue(e.ApiContext.Action, out handler))
                {
                    e.ApiContext.ResultCode = ApiResultCode.ActionNotSupported;
                    return;
                }
            }

            try
            {
                handler(e.ApiContext);
            }
            catch (Exception exception)
            {
                e.ApiContext.ResultCode = ApiResultCode.UnhandledException;
                e.ApiContext.Result = ExceptionSerializer.SerializeException(exception);
            }
            finally
            {
                e.IsHandled = true;
            }
        }

        private static string GenerateHash(string input)
        {
            var buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            var hashBuffer = HashAlgorithm.HashData(buffer);

            return CryptographicBuffer.EncodeToBase64String(hashBuffer);
        }
    }
}
