using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.Devices
{
    public class DeviceService : ServiceBase, IDeviceService
    {
        private readonly IApiService _apiService;
        private readonly DeviceCollection _devices = new DeviceCollection();

        public DeviceService(
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService,
            IApiService apiService)
        {
            _apiService = apiService;
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            systemEventsService.StartupCompleted += (s, e) =>
            {
                systemInformationService.Set("Devices/Count", _devices.GetAll().Count);
            };
        }

        public void AddDevice(IDevice device)
        {
            _devices.AddUnique(device.Id, device);

            _apiService.Route($"device/{device.Id}", device.HandleApiCall);
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
