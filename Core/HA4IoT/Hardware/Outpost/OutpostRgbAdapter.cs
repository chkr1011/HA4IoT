using System;
using System.Text;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Services;

namespace HA4IoT.Hardware.Outpost
{
    public class OutpostRgbAdapter : IBinaryOutputAdapter
    {
        private readonly string _deviceName;
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;

        public OutpostRgbAdapter(string deviceName, IDeviceMessageBrokerService deviceMessageBroker)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));
            if (deviceMessageBroker == null) throw new ArgumentNullException(nameof(deviceMessageBroker));

            _deviceName = deviceName;
            _deviceMessageBroker = deviceMessageBroker;
        }

        public void SetColor(int r, int g, int b)
        {
            _deviceMessageBroker.Publish($"HA4IoT/Device/{_deviceName}/Command/RGB/Set", Encoding.UTF8.GetBytes(r + "," + g + "," + b), MqttQosLevel.AtMostOnce);
        }

        public void TurnOn(params IHardwareParameter[] parameters)
        {
            SetColor(1023, 1023, 1023);
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            SetColor(0, 0, 0);
        }
    }
}
