using HA4IoT.Contracts.Hardware.Services;

namespace HA4IoT.Contracts.Hardware.DeviceMessaging
{
    public class DeviceMessage
    {
        public string Topic { get; set; }

        public byte[] Payload { get; set; }

        public MqttQosLevel QosLevel { get; set; }
    }
}
