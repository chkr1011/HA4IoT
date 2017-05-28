using System;
using System.Diagnostics;
using HA4IoT.Contracts.Devices;
using HA4IoT.Contracts.Devices.Configuration;
using HA4IoT.Contracts.Hardware.Interrupts;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsDeviceFactory : IDeviceFactory
    {
        private readonly IInterruptMonitorService _interruptMonitorService;
        private readonly CCToolsDeviceService _ccToolsDeviceService;

        public CCToolsDeviceFactory(CCToolsDeviceService ccToolsDeviceService, IInterruptMonitorService interruptMonitorService)
        {
            _interruptMonitorService = interruptMonitorService ?? throw new ArgumentNullException(nameof(interruptMonitorService));
            _ccToolsDeviceService = ccToolsDeviceService ?? throw new ArgumentNullException(nameof(ccToolsDeviceService));
        }

        public bool TryCreateDevice(string id, DeviceConfiguration deviceConfiguration, out IDevice device)
        {
            if (deviceConfiguration == null) throw new ArgumentNullException(nameof(deviceConfiguration));

            device = null;
            var driverConfiguration = deviceConfiguration.Driver.Parameters.ToObject<CCToolsDeviceDriverConfiguration>();

            switch (deviceConfiguration.Driver.Type)
            {
                case "CCTools.HSPE16_InputOnly":
                    {
                        device = _ccToolsDeviceService.CreateDevice(CCToolsDeviceType.HSPE16_InputOnly, id, driverConfiguration.Address);
                        break;
                    }

                case "CCTools.HSPE16_OutputOnly":
                    {
                        device = _ccToolsDeviceService.CreateDevice(CCToolsDeviceType.HSPE16_OutputOnly, id, driverConfiguration.Address);
                        break;
                    }

                case "CCTools.HSPE8_InputOnly":
                    {
                        device = _ccToolsDeviceService.CreateDevice(CCToolsDeviceType.HSPE8_InputOnly, id, driverConfiguration.Address);
                        break;
                    }

                case "CCTools.HSPE8_OutputOnly":
                    {
                        device = _ccToolsDeviceService.CreateDevice(CCToolsDeviceType.HSPE8_OutputOnly, id, driverConfiguration.Address);
                        break;
                    }

                case "CCTools.HSRel5":
                    {
                        device = _ccToolsDeviceService.CreateDevice(CCToolsDeviceType.HSRel5, id, driverConfiguration.Address);
                        break;
                    }

                case "CCTools.HSRel8":
                    {
                        device = _ccToolsDeviceService.CreateDevice(CCToolsDeviceType.HSRel8, id, driverConfiguration.Address);
                        break;
                    }

                case "CCTools.HSRT16":
                    {
                        device = _ccToolsDeviceService.CreateDevice(CCToolsDeviceType.HSRT16, id, driverConfiguration.Address);
                        break;
                    }
            }

            if (device != null)
            {
                if (!string.IsNullOrEmpty(driverConfiguration.Interrupt))
                {
                    var inputDevice = (CCToolsInputDeviceBase)device;
                    _interruptMonitorService.RegisterCallback(driverConfiguration.Interrupt, () =>
                    {
                        var stopwatch = Stopwatch.StartNew();
                        inputDevice.FetchState();
                        stopwatch.Stop();

                        if (stopwatch.ElapsedMilliseconds > driverConfiguration.PollDurationWarningThreshold)
                        {
                            Log.Default.Warning($"Polling device '{inputDevice.Id}' took {stopwatch.ElapsedMilliseconds} ms.");
                        }
                    });
                }

                return true;
            }

            return false;
        }
    }
}
