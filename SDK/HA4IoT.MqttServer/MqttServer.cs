using System;
using uPLibrary.Networking.M2Mqtt;

namespace HA4IoT.MqttServer
{
    public class MqttServer
    {
        private readonly MqttBroker _mqttBroker;

        public MqttServer(MqttServerChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            _mqttBroker = new MqttBroker(channel, MqttSettings.Instance);
        }

        public void Start()
        {
            _mqttBroker.Start();
        }
    }
}
