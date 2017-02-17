using System.Collections.Generic;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Services.System
{
    public interface IDeviceRegistryService : IService
    {
        void AddDevice(IDevice device);

        TDevice GetDevice<TDevice>(string id) where TDevice : IDevice;

        TDevice GetDevice<TDevice>() where TDevice : IDevice;

        IList<TDevice> GetDevices<TDevice>() where TDevice : IDevice;

        IList<IDevice> GetDevices();
    }
}
