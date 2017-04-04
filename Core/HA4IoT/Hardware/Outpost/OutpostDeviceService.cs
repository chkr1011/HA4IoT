using System;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.Outpost
{
    public class OutpostDeviceService : ServiceBase
    {
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;

        public OutpostDeviceService(IDeviceMessageBrokerService deviceMessageBroker)
        {
            if (deviceMessageBroker == null) throw new ArgumentNullException(nameof(deviceMessageBroker));

            _deviceMessageBroker = deviceMessageBroker;
        }

        public OutpostRgbAdapter GetRgbAdapter(string deviceName)
        {
            return new OutpostRgbAdapter(deviceName, _deviceMessageBroker);
        }

        public OutpostLpdAdapter GetLpdAdapter(string deviceName)
        {
            throw new NotImplementedException();
        }
    }
}
