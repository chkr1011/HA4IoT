using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Scripting.Proxies;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Scripting
{
    [ApiServiceClass(typeof(IScriptingService))]
    public class ScriptingService : ServiceBase, IScriptingService
    {
        private readonly IMessageBrokerService _messageBroker;
        private readonly IComponentRegistryService _componentRegistryService;
        private readonly ISchedulerService _schedulerService;
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;
        private readonly IEnumerable<IScriptProxy> _scriptProxies;
        private readonly ILogger _log;

        public ScriptingService(
            IEnumerable<IScriptProxy> scriptProxies, 
            IApiDispatcherService apiDispatcherService, 
            ISchedulerService schedulerService,
            IComponentRegistryService componentRegistryService,
            IMessageBrokerService messageBroker,
            IDeviceMessageBrokerService deviceMessageBroker,
            ILogService logService)
        {
            if (apiDispatcherService == null) throw new ArgumentNullException(nameof(apiDispatcherService));

            _deviceMessageBroker = deviceMessageBroker ?? throw new ArgumentNullException(nameof(deviceMessageBroker));
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _componentRegistryService = componentRegistryService ?? throw new ArgumentNullException(nameof(componentRegistryService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _scriptProxies = scriptProxies ?? throw new ArgumentNullException(nameof(scriptProxies));
            _log = logService?.CreatePublisher(nameof(ScriptingService)) ?? throw new ArgumentNullException(nameof(logService));

            //apiDispatcherService.RegisterAdapter(this);

            UserData.RegisterType<DebuggingScriptProxy>();
            UserData.RegisterType<SchedulerScriptProxy>();
            UserData.RegisterType<ComponentScriptProxy>();
            UserData.RegisterType<messageBrokerProxy>();
            UserData.RegisterType<MqttScriptProxy>();

            foreach (var scriptProxy in scriptProxies)
            {
                UserData.RegisterType(scriptProxy.GetType());
            }
        }

        //public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public ScriptExecutionResult ExecuteScript(string script, string entryFunctionName)
        {
            if (script == null) throw new ArgumentNullException(nameof(script));

            var result = ExecuteScriptInternal(script, entryFunctionName);
            if (result.Exception != null)
            {
                throw new ScriptingException("Error while executing script.", result.Exception);
            }

            return result;
        }

        public bool TryExecuteScript(string script, out ScriptExecutionResult result, string entryFunctionName = null)
        {
            if (script == null) throw new ArgumentNullException(nameof(script));

            result = ExecuteScriptInternal(script, entryFunctionName);
            return result.Exception == null;
        }

        public IScriptingSession CreateScriptingSession(string scriptCode)
        {
            var scriptingSession = new ScriptingSession(scriptCode, _log);
            ////RegisterApiFunctions(scriptingSession.Script);

            scriptingSession.RegisterProxies(new DebuggingScriptProxy(_log, scriptingSession));
            scriptingSession.RegisterProxies(new SchedulerScriptProxy(_schedulerService, scriptingSession));
            scriptingSession.RegisterProxies(new ComponentScriptProxy(_componentRegistryService, scriptingSession));
            scriptingSession.RegisterProxies(new messageBrokerProxy(_messageBroker, scriptingSession));
            scriptingSession.RegisterProxies(new MqttScriptProxy(_deviceMessageBroker, scriptingSession));
            scriptingSession.RegisterProxies(_scriptProxies.ToArray());

            return scriptingSession;
        }

        ////public void NotifyStateChanged(IComponent component)
        ////{
        ////}

        [ApiMethod]
        public void ExecuteScriptCode(IApiCall apiCall)
        {
            var request = apiCall.Parameter.ToObject<ExecuteScriptCodeRequest>();
            if (request == null)
            {
                apiCall.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            var result = ExecuteScriptInternal(request.ScriptCode, request.EntryMethodName);
            apiCall.Result = JObject.FromObject(result);
        }

        [ApiMethod]
        public void GetProxyMethods(IApiCall apiCall)
        {
            var scriptingSession = CreateScriptingSession("return nil");

            var rootJson = new JObject();
            foreach (var proxy in scriptingSession.Proxies)
            {
                var proxyJson = new JObject();
                foreach (var method in proxy.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    var parametersJson = new JObject();
                    foreach (var parameter in method.GetParameters())
                    {
                        parametersJson[parameter.Name] = new JObject
                        {
                            ["Type"] = ConvertTypeToString(parameter.ParameterType)
                        };
                    }

                    var methodJson = new JObject
                    {
                        ["Parameters"] = parametersJson,
                        ["ReturnType"] = ConvertTypeToString(method.ReturnType)
                    };

                    var methodName = char.ToLower(method.Name[0]) + method.Name.Substring(1);
                    proxyJson[methodName] = methodJson;
                }

                rootJson[proxy.Name] = proxyJson;
            }

            var request = apiCall.Parameter.ToObject<ExecuteScriptCodeRequest>();
            if (request == null)
            {
                apiCall.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            apiCall.Result = rootJson;
        }

        private string ConvertTypeToString(Type type)
        {
            var result = type.Name.ToLower();


            return result;
        }

        private ScriptExecutionResult ExecuteScriptInternal(string scriptCode, string entryFunctionName)
        {
            var scriptingSession = CreateScriptingSession(scriptCode);
            return scriptingSession.Execute(entryFunctionName);
        }

        ////private void RegisterApiFunctions(Script script)
        ////{
        ////    script.Globals["api_execute"] = (Func<string, string, string>)((a, p) =>
        ////    {
        ////        var parameter = string.IsNullOrEmpty(p) ? new JObject() : JObject.Parse(p);
        ////        var apiCall = new ApiCall(a, parameter, null);
        ////        RequestReceived?.Invoke(this, new ApiRequestReceivedEventArgs(apiCall));

        ////        return apiCall.Result.ToString();
        ////    });
        ////}
    }
}