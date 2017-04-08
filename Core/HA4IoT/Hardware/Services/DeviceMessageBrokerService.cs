using System;
using System.Text;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using MQTTnet;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using MQTTnet.Core.Packets;
using MQTTnet.Core.Protocol;
using MQTTnet.Core.Server;

namespace HA4IoT.Hardware.Services
{
    [ApiServiceClass(typeof(IDeviceMessageBrokerService))]
    public class DeviceMessageBrokerService : ServiceBase, IDeviceMessageBrokerService
    {
        private readonly MqttServer _server;
        private readonly MqttClient _client;
        private readonly ILogger _log;
        private readonly MqttCommunicationAdapter _clientCommunicationAdapter;

        public DeviceMessageBrokerService(ILogService logService)
        {
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            _log = logService.CreatePublisher(nameof(DeviceMessageBrokerService));

            var channelA = new MqttCommunicationAdapter();
            _clientCommunicationAdapter = new MqttCommunicationAdapter();
            channelA.Partner = _clientCommunicationAdapter;
            _clientCommunicationAdapter.Partner = channelA;

            var mqttClientOptions = new MqttClientOptions { ClientId = "HA4IoT.Loopback" };
            _client = new MqttClient(mqttClientOptions, channelA);
            _client.ApplicationMessageReceived += ProcessIncomingMessage;

            var mqttServerOptions = new MqttServerOptions();
            _server = new MqttServerFactory().CreateMqttServer(mqttServerOptions);
            _server.ClientConnected += (s, e) => _log.Info($"MQTT client '{e.Identifier}' connected.");
        }

        public event EventHandler<DeviceMessageReceivedEventArgs> MessageReceived;

        public void Initialize()
        {
            _server.Start();
            _server.InjectClient("HA4IoT.Loopback", _clientCommunicationAdapter);

            _client.ConnectAsync().Wait();
            _client.SubscribeAsync(new TopicFilter("#", MqttQualityOfServiceLevel.AtMostOnce)).Wait();

            _log.Info("MQTT client (loopback) connected.");
        }

        [ApiMethod]
        public void Publish(IApiContext apiContext)
        {
            var deviceMessage = apiContext.Parameter.ToObject<DeviceMessage>();
            Publish(deviceMessage.Topic, deviceMessage.Payload, deviceMessage.QosLevel);
        }

        public void Publish(string topic, byte[] payload, MqttQosLevel qosLevel)
        {
            _client.PublishAsync(new MqttApplicationMessage(topic, payload, (MqttQualityOfServiceLevel)qosLevel, false)).Wait();
            _log.Verbose($"Published message '{topic}' [{Encoding.UTF8.GetString(payload)}].");
        }

        public void Subscribe(string topicPattern, Action<DeviceMessage> callback)
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

        private void ProcessIncomingMessage(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            _log.Verbose($"Broker received message '{e.ApplicationMessage.Topic}' [{Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}].");

            var message = new DeviceMessage
            {
                Topic = e.ApplicationMessage.Topic,
                Payload = e.ApplicationMessage.Payload,
                QosLevel = (MqttQosLevel)e.ApplicationMessage.QualityOfServiceLevel
            };

            MessageReceived?.Invoke(this, new DeviceMessageReceivedEventArgs(message));
        }
    }
}
