using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware.Mqtt;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace HA4IoT.Hardware.Mqtt
{
    [ApiServiceClass(typeof(IMqttService))]
    public class MqttService : ServiceBase, IMqttService
    {
        private readonly MqttBrokerChannel _brokerChannel = new MqttBrokerChannel();
        private readonly MqttBroker _broker;
        
        private readonly MqttClient _client;

        public MqttService()
        {
            _broker = new MqttBroker(_brokerChannel, MqttSettings.Instance);
            _client = new MqttClient(new MqttLoopbackClientChannel());
        }

        public override void Startup()
        {
            _broker.Start();
            Log.Info("MQTT broker started.");

            _brokerChannel.Attach(_client);
            Log.Info("MQTT client (loopback) connected.");

            _client.MqttMsgPublishReceived += ProcessIncomingMessage;
            
            //_client.Subscribe(new[] { "SonoffPow_01" }, new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
        }

        [ApiMethod]
        public void Publish(IApiContext apiContext)
        {
            var parameters = apiContext.Parameter.ToObject<PublishMqttMessageParameter>();
            this.Publish(parameters);
        }

        public ushort Publish(string topic, byte[] message, MqttQosLevel qosLevel = MqttQosLevel.At_Least_Once)
        {
            var messageId = _client.Publish(topic, message, (byte)qosLevel, false);
            Log.Verbose($"Published MQTT message for topic '{topic}'.");

            return messageId;
        }

        private void ProcessIncomingMessage(object sender, MqttMsgPublishEventArgs e)
        {
            Log.Verbose($"Received MQTT message for topic '{e.Topic}'.");
        }
    }
}
