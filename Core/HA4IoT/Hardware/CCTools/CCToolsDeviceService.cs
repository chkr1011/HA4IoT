using System;
using System.Diagnostics;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Devices;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Hardware.Interrupts;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using HA4IoT.Hardware.CCTools.Devices;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsDeviceService : ServiceBase
    {
        private readonly object _syncRoot = new object();
        private readonly II2CBusService _i2CBusService;
        private readonly IDeviceRegistryService _deviceRegistryService;
        private readonly ILogger _log;

        public CCToolsDeviceService(
            IDeviceRegistryService deviceRegistryService,
            II2CBusService i2CBusService,
            IInterruptMonitorService interruptMonitorService,
            IScriptingService scriptingService,
            ILogService log)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            _deviceRegistryService = deviceRegistryService ?? throw new ArgumentNullException(nameof(deviceRegistryService));
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _log = log?.CreatePublisher(nameof(CCToolsDeviceService)) ?? throw new ArgumentNullException(nameof(log));

            scriptingService.RegisterScriptProxy(s => new CCToolsDeviceScriptProxy(this));
            deviceRegistryService.RegisterDeviceFactory(new CCToolsDeviceFactory(this, interruptMonitorService));
        }

        public IDevice RegisterDevice(CCToolsDeviceType type, string id, int address)
        {
            var device = CreateDevice(type, id, address);
            _deviceRegistryService.RegisterDevice(device);
            return device;
        }

        public IDevice CreateDevice(CCToolsDeviceType type, string id, int address)
        {
            var i2CSlaveAddress = new I2CSlaveAddress(address);
            IDevice deviceInstance;
            switch (type)
            {
                case CCToolsDeviceType.HSPE16_InputOnly:
                    {
                        deviceInstance = new HSPE16InputOnly(id, i2CSlaveAddress, _i2CBusService, _log)
                        {
                            AutomaticallyFetchState = true
                        };
                        break;
                    }

                case CCToolsDeviceType.HSPE16_OutputOnly:
                    {
                        deviceInstance = new HSPE16OutputOnly(id, i2CSlaveAddress, _i2CBusService, _log);
                        break;
                    }

                case CCToolsDeviceType.HSPE8_InputOnly:
                    {
                        deviceInstance = new HSPE8InputOnly(id, i2CSlaveAddress, _i2CBusService, _log)
                        {
                            AutomaticallyFetchState = true
                        };
                        break;
                    }

                case CCToolsDeviceType.HSPE8_OutputOnly:
                    {
                        deviceInstance = new HSPE8OutputOnly(id, i2CSlaveAddress, _i2CBusService, _log);
                        break;
                    }

                case CCToolsDeviceType.HSRel5:
                    {
                        deviceInstance = new HSREL5(id, i2CSlaveAddress, _i2CBusService, _log);
                        break;
                    }

                case CCToolsDeviceType.HSRel8:
                    {
                        deviceInstance = new HSREL8(id, i2CSlaveAddress, _i2CBusService, _log);
                        break;
                    }

                case CCToolsDeviceType.HSRT16:
                    {
                        deviceInstance = new HSRT16(id, i2CSlaveAddress, _i2CBusService, _log);
                        break;
                    }

                default: throw new NotSupportedException();
            }

            return deviceInstance;
        }

        public void PollAllInputDevices()
        {
            var inputDevices = _deviceRegistryService.GetDevices<CCToolsInputDeviceBase>();
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
