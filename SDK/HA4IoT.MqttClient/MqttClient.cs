using System;
using System.Reflection;
using HA4IoT.Contracts.Hardware.Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace HA4IoT.MqttClient
{
    public class MqttClient
    {
        private readonly uPLibrary.Networking.M2Mqtt.MqttClient _mqttClient = new uPLibrary.Networking.M2Mqtt.MqttClient("...");
        
        public MqttClient(MqttInMemoryChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            _mqttClient.MqttMsgPublishReceived += ForwardReceivedMessage;

            // Inject the transport channel because the constructor is not available.
            // https://github.com/eclipse/paho.mqtt.m2mqtt/pull/39
            var clientChannel = new MqttClientChannel(channel);

            var channelField = _mqttClient.GetType().GetField("channel", BindingFlags.NonPublic | BindingFlags.Instance);
            channelField.SetValue(_mqttClient, clientChannel);
        }

        public event EventHandler<MqttMessageReceivedEventArgs> MessageReceived;

        public void Connect(string clientId)
        {
            _mqttClient.Connect(clientId);
        }

        public ushort Publish(string topic, byte[] message, byte qosLevel, bool retain)
        {
            return _mqttClient.Publish(topic, message, qosLevel, retain);
        }

        public ushort Subscribe(string[] topics, byte[] qosLevels)
        {
            return _mqttClient.Subscribe(topics, qosLevels);
        }

        private void ForwardReceivedMessage(object sender, MqttMsgPublishEventArgs e)
        {
            MessageReceived?.Invoke(this, new MqttMessageReceivedEventArgs
            {
                Topic = e.Topic,
                Payload = e.Message,
                QosLevel = e.QosLevel
            });
        }
    }
}
