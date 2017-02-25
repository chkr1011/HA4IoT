using HA4IoT.Contracts.Hardware.Services;

namespace HA4IoT.Contracts.Hardware.Mqtt
{
    public class PublishMqttMessageParameter
    {
        public string Topic { get; set; }

        public byte[] Message { get; set; }

        public MqttQosLevel QosLevel { get; set; } = MqttQosLevel.At_Most_Once;
    }
}
