using HA4IoT.Contracts.Hardware.Mqtt;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HA4IoT.Contracts.Hardware.DeviceMessaging
{
    public class DeviceMessage
    {
        public string Topic { get; set; }

        public byte[] Payload { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MqttQosLevel QosLevel { get; set; }
    }
}
