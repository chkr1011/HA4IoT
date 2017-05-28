using System.Collections.Generic;

namespace HA4IoT.Contracts.Devices.Configuration
{
    public class DeviceRegistryServiceConfiguration
    {
        public Dictionary<string, DeviceConfiguration> Devices { get; set; } = new Dictionary<string, DeviceConfiguration>();
    }
}
