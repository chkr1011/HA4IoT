using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Scripting.Configuration;
using HA4IoT.Contracts.Services;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Scripting
{
    [ApiServiceClass(typeof(IScriptingService))]
    public class ScriptingService : ServiceBase, IScriptingService
    {
        private readonly List<Func<IScriptingSession, IScriptProxy>> _scriptProxyCreators = new List<Func<IScriptingSession, IScriptProxy>>();
        private readonly IConfigurationService _configurationService;
        private readonly ILogger _log;
        
        public ScriptingService(IConfigurationService configurationService, ILogService logService)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            if (configurationService == null) throw new ArgumentNullException(nameof(configurationService));
            _log = logService?.CreatePublisher(nameof(ScriptingService)) ?? throw new ArgumentNullException(nameof(logService));

            UserData.RegisterType<DebuggingScriptProxy>();
            RegisterScriptProxy(s => new DebuggingScriptProxy(_log, s));
        }

        public override void Startup()
        {
            if (!Directory.Exists(StoragePath.ScriptsRoot))
            {
                Directory.CreateDirectory(StoragePath.ScriptsRoot);
            }
        }

        public ScriptExecutionResult ExecuteScriptFile(string name, string entryFunctionName)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var result = ExecuteScriptFileInternal(name, entryFunctionName);
            if (result.Exception != null)
            {
                throw new ScriptingException("Error while executing script.", result.Exception);
            }

            return result;
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

        public bool TryExecuteScriptFile(string name, out ScriptExecutionResult result, string entryFunctionName = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            result = ExecuteScriptFileInternal(name, entryFunctionName);
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

        public void TryExecuteStartupScripts()
        {
            var configuration = _configurationService.GetConfiguration<ScriptingServiceConfiguration>("ScriptingService");
            foreach (var startupScript in configuration.StartupScripts)
            {
                ScriptExecutionResult result;
                TryExecuteScriptFile(startupScript.Name, out result, startupScript.EntryFunction);
            }
        }

        private string ConvertTypeToString(Type type)
        {
            var result = type.Name.ToLower();


            return result;
        }

        private ScriptExecutionResult ExecuteScriptFileInternal(string name, string entryFunctionName)
        {
            var filename = Path.Combine(StoragePath.ScriptsRoot, name + ".lua");
            if (!File.Exists(filename))
            {
                return new ScriptExecutionResult
                {
                    Exception = new ScriptingException("Script file not found.", null)
                };
            }

            var scriptCode = File.ReadAllText(filename);
            var scriptingSession = CreateScriptingSession(scriptCode);
            return scriptingSession.Execute(entryFunctionName);
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