using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware
{
    public static class ControllerExtensions
    {
        public static TDevice Device<TDevice>(this IDeviceController controller, Enum id) where TDevice : IDevice
        {
            return controller.GetDevice<TDevice>(DeviceIdFactory.CreateIdFrom(id));
        }
    }
}
