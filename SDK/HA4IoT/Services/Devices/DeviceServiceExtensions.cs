using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware;

namespace HA4IoT.Services.Devices
{
    public static class DeviceServiceExtensions
    {
        public static TDevice GetDevice<TDevice>(this IDeviceService deviceService, Enum id) where TDevice : IDevice
        {
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));

            var deviceId = DeviceIdFactory.CreateIdFrom(id);
            return deviceService.GetDevice<TDevice>(deviceId);
        }
    }
}
