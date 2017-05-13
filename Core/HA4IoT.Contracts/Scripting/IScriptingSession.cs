using System.Collections.Generic;
using System.Text;

namespace HA4IoT.Contracts.Scripting
{
    public interface IScriptingSession
    {
        StringBuilder DebugOutput { get; }

        IList<IScriptProxy> Proxies { get; }

        ScriptExecutionResult Execute(string entryFunctionName = null);
    }
}
