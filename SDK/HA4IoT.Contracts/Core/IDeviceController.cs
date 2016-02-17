using System.Collections.Generic;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Core
{
    public interface IDeviceController
    {
        void AddDevice(IDevice device);

        TDevice Device<TDevice>(DeviceId id) where TDevice : IDevice;

        TDevice Device<TDevice>() where TDevice : IDevice;

        IList<TDevice> Devices<TDevice>() where TDevice : IDevice;

        IList<IDevice> Devices();
    }
}
