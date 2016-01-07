using System;

namespace HA4IoT.Contracts.Hardware
{
    public static class DeviceIdExtensions
    {
        public static DeviceId ToDeviceId(this string value)
        {
            return new DeviceId(value);
        }

        public static DeviceId ToDeviceId(this Enum value)
        {
            return value.ToString().ToDeviceId();
        }
    }
}
