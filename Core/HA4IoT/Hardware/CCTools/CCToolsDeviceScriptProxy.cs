using System;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsDeviceScriptProxy : IScriptProxy
    {
        private readonly CCToolsDeviceService _ccToolsDeviceService;

        [MoonSharpHidden]
        public CCToolsDeviceScriptProxy(CCToolsDeviceService ccToolsDeviceService)
        {
            _ccToolsDeviceService = ccToolsDeviceService ?? throw new ArgumentNullException(nameof(ccToolsDeviceService));
        }

        [MoonSharpHidden]
        public string Name => "cctools";

        public void Register(string type, string id, int address)
        {
            var deviceType = (CCToolsDevice)Enum.Parse(typeof(CCToolsDevice), type, true);
            _ccToolsDeviceService.RegisterDevice(deviceType, id, address);
        }
    }
}
