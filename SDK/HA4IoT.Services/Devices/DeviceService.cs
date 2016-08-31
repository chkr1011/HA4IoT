using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.Devices
{
    public class DeviceService : ServiceBase, IDeviceService
    {
        private readonly DeviceCollection _devices = new DeviceCollection();

        public DeviceService(
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService)
        {
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            systemEventsService.StartupCompleted += (s, e) =>
            {
                systemInformationService.Set("Devices/Count", _devices.GetAll().Count);
            };
        }

        public void AddDevice(IDevice device)
        {
            _devices.AddUnique(device.Id, device);
        }

        public TDevice GetDevice<TDevice>(DeviceId id) where TDevice : IDevice
        {
            return _devices.Get<TDevice>(id);
        }

        public TDevice GetDevice<TDevice>() where TDevice : IDevice
        {
            return _devices.Get<TDevice>();
        }

        public IList<TDevice> GetDevices<TDevice>() where TDevice : IDevice
        {
            return _devices.GetAll<TDevice>();
        }

        public IList<IDevice> GetDevices()
        {
            return _devices.GetAll();
        }
    }
}
