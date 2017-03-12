using System;
using System.Text;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Mqtt;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.MqttServer;

namespace HA4IoT.Hardware.Services
{
    [ApiServiceClass(typeof(IDeviceMessageBrokerService))]
    public class DeviceMessageBrokerService : ServiceBase, IDeviceMessageBrokerService
    {
        private readonly MqttServer.MqttServer _server;
        private readonly MqttClient.MqttClient _client;

        private readonly ILogger _log;

        public DeviceMessageBrokerService(ILogService logService)
        {
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            _log = logService.CreatePublisher(nameof(DeviceMessageBrokerService));

            var channelA = new MqttInMemoryChannel();
            var channelB = new MqttInMemoryChannel();
            channelA.Partner = channelB;
            channelB.Partner = channelA;

            _client = new MqttClient.MqttClient(channelA);
            _client.MessageReceived += ProcessIncomingMessage;

            var brokerChannel = new MqttServerChannel(_log);
            _server = new MqttServer.MqttServer(brokerChannel);
            brokerChannel.Attach(channelB);
        }

        public event EventHandler<DeviceMessageReceivedEventArgs> MessageReceived;

        public override void Startup()
        {
            _server.Start();

            _client.Connect("HA4IoT.Loopback");
            _client.Subscribe(new[] { "#" }, new[] { (byte)MqttQosLevel.AtLeastOnce });

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
            _client.Publish(topic, payload, (byte)qosLevel, false);
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

        private void ProcessIncomingMessage(object sender, MqttMessageReceivedEventArgs e)
        {
            _log.Verbose($"Broker received message '{e.Topic}' [{Encoding.UTF8.GetString(e.Payload)}].");

            var message = new DeviceMessage
            {
                Topic = e.Topic,
                Payload = e.Payload,
                QosLevel = (MqttQosLevel)e.QosLevel
            };

            MessageReceived?.Invoke(this, new DeviceMessageReceivedEventArgs(message));
        }
    }
}
