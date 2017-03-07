using HA4IoT.Contracts.Hardware.Services;

namespace HA4IoT.Contracts.Hardware.Mqtt
{
    public class MqttMessage
    {
        public string Topic { get; set; }

        public byte[] Message { get; set; }

        public MqttQosLevel QosLevel { get; set; }
    }
}
