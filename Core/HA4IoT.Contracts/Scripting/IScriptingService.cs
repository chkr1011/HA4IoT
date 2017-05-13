using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Scripting
{
    public interface IScriptingService : IService
    {
        IScriptingSession CreateScriptingSession(string scriptCode);

        ScriptExecutionResult ExecuteScript(string scriptCode, string entryFunctionName = null);

        bool TryExecuteScript(string scriptCode, out ScriptExecutionResult result, string entryFunctionName = null);      
    }
}
