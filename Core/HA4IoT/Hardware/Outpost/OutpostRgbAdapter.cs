using System;
using System.Text;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Services;

namespace HA4IoT.Hardware.Outpost
{
    public class OutpostRgbAdapter : IBinaryOutputAdapter, ILampAdapter
    {
        private readonly string _topic;
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;

        public OutpostRgbAdapter(string topic, IDeviceMessageBrokerService deviceMessageBroker)
        {
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
            _deviceMessageBroker = deviceMessageBroker ?? throw new ArgumentNullException(nameof(deviceMessageBroker));
        }

        public bool SupportsColor => true;

        public int ColorResolutionBits => 10;

        public void SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            SetState(powerState, null, parameters);
        }

        public void SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters)
        {
            if (!SupportsColor && color != null)
            {
                throw new InvalidOperationException("Color is not supported by adapter.");
            }

            var r = 0;
            var g = 0;
            var b = 0;

            if (powerState == AdapterPowerState.On && color == null)
            {
                r = 1023;
                g = 1023;
                b = 1023;
            }
            else if (powerState == AdapterPowerState.On && color != null)
            {
                r = color.Red;
                g = color.Green;
                b = color.Blue;
            }

            _deviceMessageBroker.Publish(_topic, Encoding.UTF8.GetBytes(r + "," + g + "," + b), MqttQosLevel.AtMostOnce);
        }
    }
}
