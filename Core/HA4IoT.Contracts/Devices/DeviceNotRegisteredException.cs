using System;

namespace HA4IoT.Contracts.Devices
{
    public class DeviceNotRegisteredException : Exception
    {
        public DeviceNotRegisteredException(string deviceId) : base("Device with ID '" + deviceId + "' is not registered.")
        {
        }
    }
}
