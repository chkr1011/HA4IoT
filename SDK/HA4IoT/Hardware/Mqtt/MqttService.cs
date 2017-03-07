extern alias mqttClient;
using System;
using System.Text;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware.Mqtt;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using mqttClient::uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;

namespace HA4IoT.Hardware.Mqtt
{
    extern alias mqttClient;

    [ApiServiceClass(typeof(IMqttService))]
    public class MqttService : ServiceBase, IMqttService
    {
        private readonly MqttBroker _broker;
        private readonly mqttClient::uPLibrary.Networking.M2Mqtt.MqttClient _client;

        private readonly ILogger _log;

        public MqttService(ILogService logService)
        {
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            _log = logService.CreatePublisher(nameof(MqttService));

            var channelA = new MqttInMemoryChannel();
            var channelB = new MqttInMemoryChannel();
            channelA.Partner = channelB;
            channelB.Partner = channelA;

            _client = new mqttClient::uPLibrary.Networking.M2Mqtt.MqttClient(new ClientMqttStream(channelA));
            _client.MqttMsgPublishReceived += ProcessIncomingMessage;

            var brokerChannel = new MqttBrokerChannel(_log);
            _broker = new MqttBroker(brokerChannel, MqttSettings.Instance);
            brokerChannel.Attach(new BrokerMqttStream(channelB));
        }

        public event EventHandler<MqttMessageReceivedEventArgs> MessageReceived;

        public override void Startup()
        {
            _broker.Start();

            _client.Connect("HA4IoT.Loopback");
            _client.Subscribe(new[] { "#" }, new[] { (byte)MqttQosLevel.AtLeastOnce });

            _log.Info("MQTT client (loopback) connected.");
        }

        [ApiMethod]
        public void Publish(IApiContext apiContext)
        {
            var parameters = apiContext.Parameter.ToObject<MqttMessage>();
            this.Publish(parameters);
        }

        public int Publish(string topic, byte[] message, MqttQosLevel qosLevel)
        {
            var messageId = _client.Publish(topic, message, (byte)qosLevel, false);
            _log.Verbose($"Published message '{topic}' [{Encoding.UTF8.GetString(message)}].");

            return messageId;
        }

        public void Subscribe(string topicPattern, Action<MqttMessage> callback)
        {
            if (topicPattern == null) throw new ArgumentNullException(nameof(topicPattern));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            MessageReceived += (s, e) =>
            {
                if (Regex.IsMatch(e.Message.Topic, topicPattern, RegexOptions.IgnoreCase))
                {
                    callback(e.Message);
                }
            };
        }

        private void ProcessIncomingMessage(object sender, MqttMsgPublishEventArgs e)
        {
            _log.Verbose($"Broker received message '{e.Topic}' [{Encoding.UTF8.GetString(e.Message)}].");

            var message = new MqttMessage
            {
                Topic = e.Topic,
                Message = e.Message,
                QosLevel = (MqttQosLevel)e.QosLevel
            };

            MessageReceived?.Invoke(this, new MqttMessageReceivedEventArgs(message));
        }
    }
}
