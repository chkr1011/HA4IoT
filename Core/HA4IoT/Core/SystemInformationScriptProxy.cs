using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace HA4IoT.Core
{
    public class SystemInformationScriptProxy : IScriptProxy
    {
        private readonly ISystemInformationService _systemInformationService;

        [MoonSharpHidden]
        public SystemInformationScriptProxy(ISystemInformationService systemInformationService)
        {
            _systemInformationService = systemInformationService ?? throw new ArgumentNullException(nameof(systemInformationService));
        }

        [MoonSharpHidden]
        public string Name => "systemInformation";

        public void Set(string key, object value)
        {
            _systemInformationService.Set(key, value);
        }

        public void Delete(string key)
        {
            _systemInformationService.Delete(key);
        }
    }
}
