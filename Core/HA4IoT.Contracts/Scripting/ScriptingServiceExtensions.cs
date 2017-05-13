using System;
using System.IO;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Scripting
{
    public static class ScriptingServiceExtensions
    {
        public static bool TryExecuteScriptCode(this IScriptingService scriptingService, string scriptCode, string entryFunctionName = null)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            if (scriptCode == null) throw new ArgumentNullException(nameof(scriptCode));

            ScriptExecutionResult _;
            return scriptingService.TryExecuteScript(scriptCode, out _, entryFunctionName);
        }

        public static bool TryExecuteScriptFile(this IScriptingService scriptingService, string scriptName, string entryFunctionName = null)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            if (scriptName == null) throw new ArgumentNullException(nameof(scriptName));

            ScriptExecutionResult _;
            return scriptingService.TryExecuteScriptFile(scriptName, out _, entryFunctionName);
        }

        public static object ExecuteScriptFile(this IScriptingService scriptingService, string scriptName, string entryFunctionName)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            if (scriptName == null) throw new ArgumentNullException(nameof(scriptName));

            var filename = Path.Combine(StoragePath.StorageRoot, scriptName + ".lua");
            var scriptCode = File.ReadAllText(filename);
            return scriptingService.ExecuteScript(scriptCode, entryFunctionName);
        }

        public static bool TryExecuteScriptFile(this IScriptingService scriptingService, string scriptName, out ScriptExecutionResult result, string entryFunctionName)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            if (scriptName == null) throw new ArgumentNullException(nameof(scriptName));

            var filename = Path.Combine(StoragePath.StorageRoot, scriptName + ".lua");
            var scriptCode = File.ReadAllText(filename);
            return scriptingService.TryExecuteScript(scriptCode, out result, entryFunctionName);
        }
    }
}
