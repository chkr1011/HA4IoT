using System;

namespace HA4IoT.Contracts.Scripting
{
    public class ScriptExecutionResult
    {
        public object Value { get; set; }

        public Exception Exception { get; set; }

        public int Duration { get; set; }

        public string Trace { get; set; }
    }
}
