using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Devices;
using HA4IoT.Contracts.Devices.Configuration;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Devices
{
    public class DeviceRegistryService : ServiceBase, IDeviceRegistryService
    {
        private readonly Dictionary<string, IDevice> _devices = new Dictionary<string, IDevice>();

        private readonly List<IDeviceFactory> _deviceFactories = new List<IDeviceFactory>();
        private readonly IConfigurationService _configurationService;
        private readonly ILogger _log;

        public DeviceRegistryService(
            IConfigurationService configurationService,
            ISystemInformationService systemInformationService,
            ILogService logService)
        {
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (logService == null) throw new ArgumentNullException(nameof(logService));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            
            systemInformationService.Set("Devices/Count", () => _devices.Count);

            _log = logService.CreatePublisher(nameof(DeviceRegistryService));
        }

        public void RegisterDeviceFactory(IDeviceFactory deviceFactory)
        {
            if (deviceFactory == null) throw new ArgumentNullException(nameof(deviceFactory));

            _deviceFactories.Add(deviceFactory);
        }

        public void RegisterDevices()
        {
            var configuration = _configurationService.GetConfiguration<DeviceRegistryServiceConfiguration>("DeviceRegistryService");
            foreach (var deviceConfiguration in configuration.Devices)
            {
                TryRegisterDevice(deviceConfiguration);
            }
        }

        public void RegisterDevice(IDevice device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));

            lock (_devices)
            {
                _devices.Add(device.Id, device);
                _log.Info($"Registered device '{device.Id}'.");
            }
        }

        public TDevice GetDevice<TDevice>(string id) where TDevice : IDevice
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            lock (_devices)
            {
                IDevice device;
                if (!_devices.TryGetValue(id, out device))
                {
                    throw new DeviceNotRegisteredException(id);
                }
                return (TDevice)device;
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

        private void TryRegisterDevice(KeyValuePair<string, DeviceConfiguration> deviceConfiguration)
        {
            try
            {
                IDevice device = null;
                foreach (var deviceFactory in _deviceFactories)
                {
                    if (deviceFactory.TryCreateDevice(deviceConfiguration.Key, deviceConfiguration.Value, out device))
                    {
                        break;
                    }
                }

                if (device == null)
                {
                    //_log.Warning($"Failed to register device '{deviceConfiguration.Key}'. The specified driver '{deviceConfiguration.Value.Driver.Id}' is not supported.");
                    return;
                }
                
                RegisterDevice(device);
            }
            catch (Exception exception)
            {
                _log.Warning(exception, $"Failed to register device '{deviceConfiguration.Key}'.");
            }
        }
    }
}
