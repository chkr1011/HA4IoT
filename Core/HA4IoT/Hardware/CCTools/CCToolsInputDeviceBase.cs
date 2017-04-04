using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.CCTools
{
    public abstract class CCToolsInputDeviceBase : CCToolsDeviceBase
    {
        protected CCToolsInputDeviceBase(string id, I2CIPortExpanderDriver portExpanderDriver, IDeviceMessageBrokerService deviceMessageBrokerService, ILogger log) 
            : base(id, portExpanderDriver, deviceMessageBrokerService, log)
        {
        }

        public bool AutomaticallyFetchState { get; set; }
    }
}
