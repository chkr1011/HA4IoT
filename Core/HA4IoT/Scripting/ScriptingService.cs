using System;
using System.Collections.Generic;
using System.Reflection;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Scripting
{
    [ApiServiceClass(typeof(IScriptingService))]
    public class ScriptingService : ServiceBase, IScriptingService
    {
        private readonly List<Func<IScriptingSession, IScriptProxy>> _scriptProxyCreators = new List<Func<IScriptingSession, IScriptProxy>>();
        private readonly ILogger _log;
        
        public ScriptingService(ILogService logService)
        {
            _log = logService?.CreatePublisher(nameof(ScriptingService)) ?? throw new ArgumentNullException(nameof(logService));

            UserData.RegisterType<DebuggingScriptProxy>();
            RegisterScriptProxy(s => new DebuggingScriptProxy(_log, s));
        }

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

        public void RegisterScriptProxy<TScriptProxy>(Func<IScriptingSession, TScriptProxy> instanceCreator) where TScriptProxy : IScriptProxy
        {
            if (instanceCreator == null) throw new ArgumentNullException(nameof(instanceCreator));

            UserData.RegisterType<TScriptProxy>();
            _scriptProxyCreators.Add(s => instanceCreator(s));
        }

        public IScriptingSession CreateScriptingSession(string scriptCode)
        {
            var scriptingSession = new ScriptingSession(scriptCode, _log);
            scriptingSession.RegisterScriptProxy(new DebuggingScriptProxy(_log, scriptingSession));
            foreach (var scriptProxyCreator in _scriptProxyCreators)
            {
                scriptingSession.RegisterScriptProxy(scriptProxyCreator(scriptingSession));
            }

            return scriptingSession;
        }

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