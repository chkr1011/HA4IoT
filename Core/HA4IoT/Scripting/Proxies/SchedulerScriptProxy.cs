using System;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services.System;
using MoonSharp.Interpreter;

namespace HA4IoT.Scripting.Proxies
{
    public class SchedulerScriptProxy : IScriptProxy
    {
        private readonly IScriptingSession _scriptingSession;
        private readonly ISchedulerService _schedulerService;

        [MoonSharpHidden]
        public SchedulerScriptProxy(ISchedulerService schedulerService, IScriptingSession scriptingSession)
        {
            _scriptingSession = scriptingSession ?? throw new ArgumentNullException(nameof(scriptingSession));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        }

        [MoonSharpHidden]
        public string Name => "scheduler";

        public void RegisterFromFunction(string scheduleName, string interval, string functionName)
        {
            _schedulerService.Register(scheduleName, TimeSpan.Parse(interval), () =>
            {
                var result = _scriptingSession.Execute(functionName);
                if (result.Exception != null)
                {
                    throw new ScriptingException("Error while executing script.", result.Exception);
                }
            });
        }

        public void Remove(string scheduleName)
        {
            _schedulerService.Remove(scheduleName);
        }
    }
}
