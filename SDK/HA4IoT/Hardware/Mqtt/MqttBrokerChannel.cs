using System;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Logging;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Communication;

namespace HA4IoT.Hardware.Mqtt
{
    public class MqttBrokerChannel : IMqttCommunicationLayer
    {
        private readonly StreamSocketListener _socket = new StreamSocketListener();

        public event MqttClientConnectedEventHandler ClientConnected;

        public void Start()
        {
            _socket.ConnectionReceived += ProcessConnection;
            _socket.BindServiceNameAsync(MqttSettings.Instance.Port.ToString()).AsTask().Wait();

            Log.Info($"Started MQTT broker with port {MqttSettings.Instance.Port}.");
        }

        public void Stop()
        {
            _socket.Dispose();
        }

        public void Attach(MqttClient mqttClient)
        {
            if (mqttClient == null) throw new ArgumentNullException(nameof(mqttClient));

            Log.Verbose("Attached internal client to MQTT broker.");
            ClientConnected?.Invoke(this, new MqttClientConnectedEventArgs(mqttClient));
        }

        private void ProcessConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Log.Verbose($"External client ({args.Socket.Information.RemoteAddress}) connected to MQTT broker.");
            var mqttClient = new MqttClient(new MqttNetworkChannel(args.Socket));
            ClientConnected?.Invoke(this, new MqttClientConnectedEventArgs(mqttClient));
        }
    }
}