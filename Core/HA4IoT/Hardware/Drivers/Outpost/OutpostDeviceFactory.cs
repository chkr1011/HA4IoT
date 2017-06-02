using System;
using HA4IoT.Contracts.Devices;
using HA4IoT.Contracts.Devices.Configuration;
using HA4IoT.Contracts.Hardware.I2C.I2CHardwareBridge.Configuration;
using HA4IoT.Contracts.Hardware.Outpost.Configuration;

namespace HA4IoT.Hardware.Drivers.Outpost
{
    public class OutpostDeviceFactory : IDeviceFactory
    {
        private readonly OutpostDeviceService _outpostDeviceService;

        public OutpostDeviceFactory(OutpostDeviceService outpostDeviceService)
        {
            _outpostDeviceService = outpostDeviceService ?? throw new ArgumentNullException(nameof(outpostDeviceService));
        }

        public bool TryCreateDevice(string id, DeviceConfiguration deviceConfiguration, out IDevice device)
        {
            switch (deviceConfiguration.Driver.Type)
            {
                case "Outpost.LpdBridge":
                {
                    return CreateGetLpdBridgeAdapter(id, deviceConfiguration, out device);
                }

                case "I2CHardwareBridge":
                {
                    return CreateI2CHardwareBridge(id, deviceConfiguration, out device);
                }
            }

            device = null;
            return false;
        }

        private bool CreateGetLpdBridgeAdapter(string id, DeviceConfiguration deviceConfiguration, out IDevice device)
        {
            var configuration = deviceConfiguration.Driver.Parameters.ToObject<LpdBridgeConfiguration>();

            device = _outpostDeviceService.CreateLpdBridgeAdapter(id, configuration.DeviceName);
            return true;
        }

        private bool CreateI2CHardwareBridge(string id, DeviceConfiguration deviceConfiguration, out IDevice device)
        {
            var configuration = deviceConfiguration.Driver.Parameters.ToObject<I2CHardwareBridgeConfiguration>();
            device = _outpostDeviceService.CreateI2CHardwareBridge(id, configuration.Address);
            return true;
        }
    }
}
