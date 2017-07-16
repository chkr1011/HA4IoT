using System.Collections.Generic;
using HA4IoT.Contracts.Devices;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public interface IDeviceRegistryService : IService
    {
        void RegisterDevice(IDevice device);

        TDevice GetDevice<TDevice>(string id) where TDevice : IDevice;

        TDevice GetDevice<TDevice>() where TDevice : IDevice;

        IList<TDevice> GetDevices<TDevice>() where TDevice : IDevice;

        void RegisterDevices();

        void RegisterDeviceFactory(IDeviceFactory deviceFactory);
    }
}
