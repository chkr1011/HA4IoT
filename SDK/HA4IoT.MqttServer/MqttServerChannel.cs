using System;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Hardware.Mqtt;
using HA4IoT.Contracts.Logging;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Communication;

namespace HA4IoT.MqttServer
{
    public class MqttServerChannel : IMqttCommunicationLayer
    {
        private readonly StreamSocketListener _socketListener = new StreamSocketListener();
        private readonly ILogger _log;

        public event MqttClientConnectedEventHandler ClientConnected;

        public MqttServerChannel(ILogger log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            _log = log;
        }

        public void Start()
        {
            const int port = 1883;

            _socketListener.ConnectionReceived += ProcessConnection;
            _socketListener.BindServiceNameAsync(port.ToString()).AsTask().Wait();

            _log.Info($"Started MQTT broker with port {port}.");
        }

        public void Stop()
        {
            _socketListener.Dispose();
        }

        public void Attach(MqttInMemoryChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            _log.Verbose("Attached internal client to MQTT broker.");

            var mqttClient = new MqttClient(new MqttServerToClientChannel(channel));
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