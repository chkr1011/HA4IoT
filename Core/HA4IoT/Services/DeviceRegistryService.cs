using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services
{
    public class DeviceRegistryService : ServiceBase, IDeviceRegistryService
    {
        private readonly Dictionary<string, IDevice> _devices = new Dictionary<string, IDevice>();

        public DeviceRegistryService(
            ISystemInformationService systemInformationService)
        {
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            systemInformationService.Set("Devices/Count", () => _devices.Count);
        }

        public void RegisterDevice(IDevice device)
        {
            lock (_devices)
            {
                _devices.Add(device.Id, device);
            }
        }

        public TDevice GetDevice<TDevice>(string id) where TDevice : IDevice
        {
            lock (_devices)
            {
                return (TDevice) _devices[id];
            }
        }

        public TDevice GetDevice<TDevice>() where TDevice : IDevice
        {
            lock (_devices)
            {
                return _devices.Values.OfType<TDevice>().SingleOrDefault();
            }
        }

        public IList<TDevice> GetDevices<TDevice>() where TDevice : IDevice
        {
            lock (_devices)
            {
                return _devices.Values.OfType<TDevice>().ToList();
            }
        }
    }
}
