using System;
using System.Diagnostics;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools.Devices;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsDeviceService : ServiceBase
    {
        private readonly II2CBusService _i2CBusService;
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly ILogger _log;

        public CCToolsDeviceService(IDeviceRegistryService deviceService, II2CBusService i2CBusService, IDeviceMessageBrokerService deviceMessageBrokerService, ILogService log)
        {
            if (i2CBusService == null) throw new ArgumentNullException(nameof(i2CBusService));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));

            _deviceService = deviceService;
            _i2CBusService = i2CBusService;
            _deviceMessageBrokerService = deviceMessageBrokerService;
            _log = log.CreatePublisher(nameof(CCToolsDeviceService));
        }

        public HSPE16InputOnly RegisterHSPE16InputOnly(string id, I2CSlaveAddress address)
        {
            var device = new HSPE16InputOnly(id, address, _i2CBusService, _deviceMessageBrokerService, _log)
            {
                AutomaticallyFetchState = true
            };

            _deviceService.AddDevice(device);

            return device;
        }

        public HSPE16OutputOnly RegisterHSPE16OutputOnly(string id, I2CSlaveAddress address)
        {
            var device = new HSPE16OutputOnly(id, address, _i2CBusService, _deviceMessageBrokerService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public HSPE8OutputOnly RegisterHSPE8OutputOnly(string id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSPE8OutputOnly(id, i2CAddress, _i2CBusService, _deviceMessageBrokerService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public HSPE8InputOnly RegisterHSPE8InputOnly(string id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSPE8InputOnly(id, i2CAddress, _i2CBusService, _deviceMessageBrokerService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public HSREL5 RegisterHSREL5(string id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSREL5(id, i2CAddress, _i2CBusService, _deviceMessageBrokerService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public HSREL8 RegisterHSREL8(string id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSREL8(id, i2CAddress, _i2CBusService, _deviceMessageBrokerService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public HSRT16 RegisterHSRT16(string id, I2CSlaveAddress address)
        {
            var device = new HSRT16(id, address, _i2CBusService, _deviceMessageBrokerService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public void PollInputBoardStates()
        {
            var stopwatch = Stopwatch.StartNew();

            var inputDevices = _deviceService.GetDevices<CCToolsInputDeviceBase>();

            foreach (var portExpanderController in inputDevices)
            {
                if (portExpanderController.AutomaticallyFetchState)
                {
                    portExpanderController.PeekState();
                }
            }

            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 25)
            {
                _log.Warning($"Fetching inputs took {stopwatch.ElapsedMilliseconds}ms.");
            }

            foreach (var portExpanderController in inputDevices)
            {
                if (portExpanderController.AutomaticallyFetchState)
                {
                    portExpanderController.FetchState();
                }
            }
        }
    }
}
