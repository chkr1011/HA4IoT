using System;
using System.Diagnostics;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsBoardService : ServiceBase
    {
        private readonly II2CBusService _i2CBusService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly ILogger _log;

        public CCToolsBoardService(IDeviceRegistryService deviceService, II2CBusService i2CBusServiceService, ILogService logService)
        {
            if (i2CBusServiceService == null) throw new ArgumentNullException(nameof(i2CBusServiceService));
            if (logService == null) throw new ArgumentNullException(nameof(logService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));

            _deviceService = deviceService;
            _i2CBusService = i2CBusServiceService;
            _log = logService.CreatePublisher(nameof(CCToolsBoardService));
        }

        public HSPE16InputOnly RegisterHSPE16InputOnly(Enum id, I2CSlaveAddress address)
        {
            var device = new HSPE16InputOnly(id.ToString(), address, _i2CBusService, _log)
            {
                AutomaticallyFetchState = true
            };

            _deviceService.AddDevice(device);

            return device;
        }

        public HSPE16OutputOnly RegisterHSPE16OutputOnly(Enum id, I2CSlaveAddress address)
        {
            var device = new HSPE16OutputOnly(id.ToString(), address, _i2CBusService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public HSPE8OutputOnly RegisterHSPE8OutputOnly(Enum id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSPE8OutputOnly(id.ToString(), i2CAddress, _i2CBusService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public HSPE8InputOnly RegisterHSPE8InputOnly(Enum id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSPE8InputOnly(id.ToString(), i2CAddress, _i2CBusService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public HSREL5 RegisterHSREL5(Enum id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSREL5(id.ToString(), i2CAddress, _i2CBusService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public HSREL8 RegisterHSREL8(Enum id, I2CSlaveAddress i2CAddress)
        {
            var device = new HSREL8(id.ToString(), i2CAddress, _i2CBusService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public HSRT16 RegisterHSRT16(Enum id, I2CSlaveAddress address)
        {
            var device = new HSRT16(id.ToString(), address, _i2CBusService, _log);
            _deviceService.AddDevice(device);

            return device;
        }

        public void PollInputBoardStates()
        {
            var stopwatch = Stopwatch.StartNew();

            var inputDevices = _deviceService.GetDevices<CCToolsInputBoardBase>();

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
