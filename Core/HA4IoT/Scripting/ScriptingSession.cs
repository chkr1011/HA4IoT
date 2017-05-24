using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace HA4IoT.Scripting
{
    public class ScriptingSession : IScriptingSession
    {
        private readonly object _syncRoot = new object();
        private readonly Script _script = new Script();
        private readonly string _scriptCode;
        private readonly ILogger _log;

        private DynValue _defaultEntryFunction;
        private bool _isInitialized;

        public ScriptingSession(string scriptCode, ILogger log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _scriptCode = scriptCode ?? throw new ArgumentNullException(nameof(scriptCode));
            _script.Options.CheckThreadAccess = false;
            _script.DebuggerEnabled = false;
        }

        public StringBuilder DebugOutput { get; } = new StringBuilder();

        public IList<IScriptProxy> Proxies { get; } = new List<IScriptProxy>();

        public ScriptExecutionResult Execute(string entryFunctionName = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ScriptExecutionResult();
            try
            {
                lock (_syncRoot)
                {
                    result.Value = ExecuteInternal(entryFunctionName)?.ToObject();
                }
            }
            catch (Exception exception)
            {
                result.Exception = exception;
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = (int)stopwatch.Elapsed.TotalMilliseconds;
                result.Trace = DebugOutput.ToString();
            }

            CreateLogEntry(result);

            return result;
        }

        public void RegisterScriptProxy(IScriptProxy scriptProxy)
        {
            if (scriptProxy == null) throw new ArgumentNullException(nameof(scriptProxy));

            _script.Globals[scriptProxy.Name] = UserData.Create(scriptProxy);
            Proxies.Add(scriptProxy);
        }

        private DynValue ExecuteInternal(string entryFunctionName)
        {
            var scriptExecuted = false;
            DynValue scriptResult = null;

            if (!_isInitialized)
            {
                _defaultEntryFunction = _script.LoadString(_scriptCode);
                scriptResult = _script.Call(_defaultEntryFunction);
                scriptExecuted = true;
            }

            _isInitialized = true;

            if (!string.IsNullOrEmpty(entryFunctionName))
            {
                var entryFunction = _script.Globals[entryFunctionName];
                if (entryFunction != null)
                {
                    scriptResult = _script.Call(entryFunction);
                }
                else
                {
                    _log.Warning($"Entry function '{entryFunctionName}' not found in LUA script.");
                }
            }
            else if (!scriptExecuted)
            {
                scriptResult = _script.Call(_defaultEntryFunction);
            }

            return scriptResult;
        }

        private void CreateLogEntry(ScriptExecutionResult result)
        {
            var message = new StringBuilder();
            var severity = LogEntrySeverity.Verbose;

            if (result.Exception == null)
            {
                message.Append("LUA script executed (");
            }
            else
            {
                severity = LogEntrySeverity.Error;
                message.Append("Failed to execute LUA script (");
            }

            message.Append("Duration=" + result.Duration + " ms ; ");

            if (result.Value == null)
            {
                message.Append("Result=<nil> ; ");
            }
            else
            {
                message.Append("Result=" + Convert.ToString(result.Value) + " ; ");
            }

            if (result.Trace.EndsWith(global::System.Environment.NewLine))
            {
                result.Trace = result.Trace.Remove(result.Trace.Length - global::System.Environment.NewLine.Length, global::System.Environment.NewLine.Length);
            }

            if (result.Trace.Length == 0)
            {
                message.Append("Trace=<none>)");
            }
            else
            {
                message.Append("Trace=" + result.Trace + ")");
            }

            _log.Publish(severity, message.ToString(), result.Exception);
        }
    }
}
