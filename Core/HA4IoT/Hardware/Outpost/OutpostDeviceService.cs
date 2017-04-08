using System;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.Outpost
{
    public class OutpostDeviceService : ServiceBase
    {
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;

        public OutpostDeviceService(IDeviceMessageBrokerService deviceMessageBroker)
        {
            _deviceMessageBroker = deviceMessageBroker ?? throw new ArgumentNullException(nameof(deviceMessageBroker));
        }

        public OutpostRgbAdapter GetRgbAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            return new OutpostRgbAdapter(deviceName, _deviceMessageBroker);
        }

        public OutpostLpdAdapter GetLpdAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            throw new NotImplementedException();
        }

        public INumericSensorAdapter GetTemperatureAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            throw new NotImplementedException();
        }

        public INumericSensorAdapter GetHumidityAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            throw new NotImplementedException();
        }
    }
}
