using System;
using System.Diagnostics;
using HA4IoT.Contracts.Hardware;
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
        private readonly object _syncRoot = new object();
        private readonly II2CBusService _i2CBusService;
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly ILogger _log;

        public CCToolsDeviceService(IDeviceRegistryService deviceService, II2CBusService i2CBusService, IDeviceMessageBrokerService deviceMessageBrokerService, ILogService log)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _deviceMessageBrokerService = deviceMessageBrokerService;
            _log = log?.CreatePublisher(nameof(CCToolsDeviceService)) ?? throw new ArgumentNullException(nameof(log));
        }

        public IDevice RegisterDevice(CCToolsDevice device, string id, int address)
        {
            var i2CSlaveAddress = new I2CSlaveAddress(address);
            IDevice deviceInstance;
            switch (device)
            {
                case CCToolsDevice.HSPE16_InputOnly:
                    {
                        deviceInstance = new HSPE16InputOnly(id, i2CSlaveAddress, _i2CBusService, _deviceMessageBrokerService, _log)
                        {
                            AutomaticallyFetchState = true
                        };
                        break;
                    }

                case CCToolsDevice.HSPE16_OutputOnly:
                    {
                        deviceInstance = new HSPE16OutputOnly(id, i2CSlaveAddress, _i2CBusService, _deviceMessageBrokerService, _log);
                        break;
                    }

                case CCToolsDevice.HSPE8_InputOnly:
                    {
                        deviceInstance = new HSPE8InputOnly(id, i2CSlaveAddress, _i2CBusService, _deviceMessageBrokerService, _log)
                        {
                            AutomaticallyFetchState = true
                        };
                        break;
                    }

                case CCToolsDevice.HSPE8_OutputOnly:
                    {
                        deviceInstance = new HSPE8OutputOnly(id, i2CSlaveAddress, _i2CBusService, _deviceMessageBrokerService, _log);
                        break;
                    }

                case CCToolsDevice.HSRel5:
                    {
                        deviceInstance = new HSREL5(id, i2CSlaveAddress, _i2CBusService, _deviceMessageBrokerService, _log);
                        break;
                    }

                case CCToolsDevice.HSRel8:
                    {
                        deviceInstance = new HSREL8(id, i2CSlaveAddress, _i2CBusService, _deviceMessageBrokerService, _log);
                        break;
                    }

                case CCToolsDevice.HSRT16:
                {
                    deviceInstance = new HSRT16(id, i2CSlaveAddress, _i2CBusService, _deviceMessageBrokerService, _log);
                    break;
                }

                default: throw new NotSupportedException();
            }

            _deviceService.RegisterDevice(deviceInstance);
            return deviceInstance;
        }

        public void PollInputs()
        {
            var inputDevices = _deviceService.GetDevices<CCToolsInputDeviceBase>();
            var stopwatch = Stopwatch.StartNew();

            lock (_syncRoot)
            {
                foreach (var device in inputDevices)
                {
                    if (device.AutomaticallyFetchState)
                    {
                        device.PeekState();
                    }
                }

                stopwatch.Stop();

                foreach (var device in inputDevices)
                {
                    if (device.AutomaticallyFetchState)
                    {
                        device.FetchState();
                    }
                }
            }

            if (stopwatch.ElapsedMilliseconds > 100)
            {
                _log.Warning($"Fetching inputs took {stopwatch.ElapsedMilliseconds}ms.");
            }
        }
    }
}
