using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.CCTools
{
    public abstract class CCToolsInputDeviceBase : CCToolsDeviceBase
    {
        protected CCToolsInputDeviceBase(string id, I2CIPortExpanderDriver portExpanderDriver, ILogger log) 
            : base(id, portExpanderDriver, log)
        {
        }

        public bool AutomaticallyFetchState { get; set; } // TODO: Remove in the future.
    }
}
