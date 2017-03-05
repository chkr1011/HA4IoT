using System;
using System.Text;
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

        private readonly ILogger _log;
        private readonly MqttBroker _broker;
        private readonly MqttClient _client;

        public MqttService(ILogService logService)
        {
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            _log = logService.CreatePublisher(nameof(MqttService));

            _broker = new MqttBroker(_brokerChannel, MqttSettings.Instance);
            _client = new MqttClient(new MqttLoopbackClientChannel());
        }

        public override void Startup()
        {
            _broker.Start();
            _brokerChannel.Attach(_client);
            _log.Info("MQTT client (loopback) connected.");

            _client.MqttMsgPublishReceived += ProcessIncomingMessage;

            _client.Subscribe(new[] { "#" },
                new[]
                {
                    MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE
                });

            _client.Connect("HA4IoT.Loopback");
        }

        [ApiMethod]
        public void Publish(IApiContext apiContext)
        {
            var parameters = apiContext.Parameter.ToObject<PublishMqttMessageParameter>();
            this.Publish(parameters);
        }

        public int Publish(string topic, byte[] message, MqttQosLevel qosLevel)
        {
            var messageId = _client.Publish(topic, message, (byte)qosLevel, false);
            _log.Verbose($"Published MQTT message for topic '{topic}'.");

            return messageId;
        }

        private void ProcessIncomingMessage(object sender, MqttMsgPublishEventArgs e)
        {
            _log.Verbose($"Received MQTT message [{Encoding.UTF8.GetString(e.Message)}] for topic '{e.Topic}'.");
        }
    }
}
