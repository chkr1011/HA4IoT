using System;
using System.Text;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Mqtt;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Devices
{
    [ApiServiceClass(typeof(IDeviceMessageBrokerService))]
    public class DeviceMessageBrokerService : ServiceBase, IDeviceMessageBrokerService
    {
        private readonly IMqttServer _server;
        private readonly ILogger _log;

        public DeviceMessageBrokerService(ILogService logService, IScriptingService scriptingService)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            _log = logService?.CreatePublisher(nameof(DeviceMessageBrokerService)) ?? throw new ArgumentNullException(nameof(logService));

            MqttNetGlobalLogger.LogMessagePublished += (s, e) =>
            {
                if (e.TraceMessage.Level == MqttNetLogLevel.Warning)
                {
                    _log.Warning(e.TraceMessage.Exception, e.TraceMessage.ToString());
                }
                else if (e.TraceMessage.Level == MqttNetLogLevel.Error)
                {
                    _log.Error(e.TraceMessage.Exception, e.TraceMessage.ToString());
                }
                else if (e.TraceMessage.Level == MqttNetLogLevel.Info)
                {
                    _log.Info(e.TraceMessage.ToString());
                }
                else if (e.TraceMessage.Level == MqttNetLogLevel.Verbose)
                {
                    _log.Verbose(e.TraceMessage.ToString());
                }
            };

            _server = new MqttFactory().CreateMqttServer();
            _server.ApplicationMessageReceived += ProcessIncomingMessage;
            _server.ClientConnected += (s, e) => _log.Info($"MQTT client '{e.Client.ClientId}' connected.");
            _server.ClientDisconnected += (s, e) => _log.Info($"MQTT client '{e.Client.ClientId}' connected.");
            _server.ClientSubscribedTopic += (s, e) => _log.Info($"MQTT client '{e.ClientId}' subscribed topic '{e.TopicFilter}'.");
            _server.ClientUnsubscribedTopic += (s, e) => _log.Info($"MQTT client '{e.ClientId}' unsubscribed topic '{e.TopicFilter}'.");

            scriptingService.RegisterScriptProxy(s => new DeviceMessageBrokerScriptProxy(this, s));
        }

        public event EventHandler<DeviceMessageReceivedEventArgs> MessageReceived;

        private bool _isInitialized;

        public void Initialize()
        {
            lock (_server)
            {
                if (_isInitialized)
                {
                    return;
                }

                _isInitialized = true;
            }

            var options = new MqttServerOptionsBuilder()
                .WithApplicationMessageInterceptor(MessageInterceptor.Intercept)
                .WithStorage(new Storage())
                .Build();

            _server.StartAsync(options).GetAwaiter().GetResult();
            _log.Info("MQTT server started.");
        }

        [ApiMethod]
        public void GetConnectedClients(IApiCall apiCall)
        {
            var connectedClients = _server.GetConnectedClientsAsync().GetAwaiter().GetResult();
            apiCall.Result["ConnectedClients"] = JToken.FromObject(connectedClients);
        }

        [ApiMethod]
        public void Publish(IApiCall apiCall)
        {
            var deviceMessage = apiCall.Parameter.ToObject<DeviceMessage>();
            Publish(deviceMessage.Topic, deviceMessage.Payload, deviceMessage.QosLevel, deviceMessage.Retain);
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

        public void Publish(string topic, byte[] payload, MqttQosLevel qosLevel, bool retain)
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qosLevel)
                    .WithRetainFlag(retain)
                    .Build();

                _server.PublishAsync(message).GetAwaiter().GetResult();

                _log.Verbose($"Published message '{topic}' [{Encoding.UTF8.GetString(payload)}].");
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Failed to publish message '{topic}' [{Encoding.UTF8.GetString(payload)}].");
            }
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
