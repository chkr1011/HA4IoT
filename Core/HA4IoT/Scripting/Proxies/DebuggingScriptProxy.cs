using System;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace HA4IoT.Scripting.Proxies
{
    public class DebuggingScriptProxy : IScriptProxy
    {
        private readonly IScriptingSession _scriptingSession;
        private readonly ILogger _log;

        [MoonSharpHidden]
        public DebuggingScriptProxy(ILogger log, IScriptingSession scriptingSession)
        {
            _scriptingSession = scriptingSession ?? throw new ArgumentNullException(nameof(scriptingSession));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [MoonSharpHidden]
        public string Name => "log";

        public void Verbose(string message)
        {
            _log.Verbose(message);
        }

        public void Info(string message)
        {
            _log.Info(message);
        }

        public void Warning(string message)
        {
            _log.Warning(message);
        }

        public void Error(string message)
        {
            _log.Error(message);
        }

        public void Trace(string message)
        {
            _scriptingSession.DebugOutput.Append(message);
        }

        public void TraceLine(string message)
        {
            _scriptingSession.DebugOutput.AppendLine(message);
        }

        public void ClearTrace()
        {
            _scriptingSession.DebugOutput.Clear();
        }
    }
}
