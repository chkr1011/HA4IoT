using HA4IoT.Contracts.Devices.Configuration;

namespace HA4IoT.Contracts.Devices
{
    public interface IDeviceFactory
    {
        bool TryCreateDevice(string id, DeviceConfiguration deviceConfiguration, out IDevice device);
    }
}
