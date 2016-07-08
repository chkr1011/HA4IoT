using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware
{
    public static class DeviceIdFactory
    {
        public static readonly DeviceId EmptyId = new DeviceId("?");

        public static DeviceId CreateIdFrom(Enum value)
        {
            return new DeviceId(value.ToString());
        }
    }
}
