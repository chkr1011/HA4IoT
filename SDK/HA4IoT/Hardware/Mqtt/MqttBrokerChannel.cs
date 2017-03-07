using System;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Logging;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Communication;

namespace HA4IoT.Hardware.Mqtt
{
    public class MqttBrokerChannel : IMqttCommunicationLayer
    {
        private readonly StreamSocketListener _socketListener = new StreamSocketListener();
        private readonly ILogger _log;

        public event MqttClientConnectedEventHandler ClientConnected;

        public MqttBrokerChannel(ILogger log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            _log = log;
        }

        public void Start()
        {
            _socketListener.ConnectionReceived += ProcessConnection;
            _socketListener.BindServiceNameAsync(MqttSettings.Instance.Port.ToString()).AsTask().Wait();

            var port = MqttSettings.Instance.Port;
            _log.Info($"Started MQTT broker with port {port}.");
        }

        public void Stop()
        {
            _socketListener.Dispose();
        }

        public void Attach(BrokerMqttStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            _log.Verbose("Attached internal client to MQTT broker.");

            var mqttClient = new MqttClient(stream);
            ClientConnected?.Invoke(this, new MqttClientConnectedEventArgs(mqttClient));
        }

        private void ProcessConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            _log.Verbose($"External client ({args.Socket.Information.RemoteAddress}) connected to MQTT broker.");

            var mqttClient = new MqttClient(new MqttNetworkChannel(args.Socket));
            ClientConnected?.Invoke(this, new MqttClientConnectedEventArgs(mqttClient));
        }
    }
}