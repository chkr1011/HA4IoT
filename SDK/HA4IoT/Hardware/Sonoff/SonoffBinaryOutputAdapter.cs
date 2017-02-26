using System;
using HA4IoT.Hardware.Mqtt;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.Sonoff
{
    public class SonoffBinaryOutputAdapter : IBinaryOutputAdapter
    {
        private readonly string _topic;
        private readonly IMqttService _mqttService;

        public SonoffBinaryOutputAdapter(string topic, IMqttService mqttService)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (mqttService == null) throw new ArgumentNullException(nameof(mqttService));

            _topic = topic;
            _mqttService = mqttService;
        }

        public void TurnOn(params IHardwareParameter[] parameters)
        {
            int messageId = _mqttService.Publish(_topic, "ON", MqttQosLevel.Exactly_Once);
            Log.Verbose("RES: " + messageId);
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            _mqttService.Publish(_topic, "OFF", MqttQosLevel.Exactly_Once);
        }
    }
}