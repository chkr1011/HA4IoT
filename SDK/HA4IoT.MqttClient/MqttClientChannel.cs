using System;
using HA4IoT.Contracts.Hardware.Mqtt;
using uPLibrary.Networking.M2Mqtt;

namespace HA4IoT.MqttClient
{
    public class MqttClientChannel : IMqttNetworkChannel
    {
        public MqttClientChannel(MqttInMemoryChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            Channel = channel;
        }

        public MqttInMemoryChannel Channel { get; }

        public bool DataAvailable => Channel.DataAvailable;

        public int Receive(byte[] buffer)
        {
            return Channel.Receive(buffer);
        }

        public int Receive(byte[] buffer, int timeout)
        {
            return Channel.Receive(buffer, timeout);
        }

        public int Send(byte[] buffer)
        {
            return Channel.Send(buffer);
        }

        public void Close()
        {
            Channel.Close();
        }

        public void Connect()
        {
            Channel.Connect();
        }

        public void Accept()
        {
            Channel.Accept();
        }
    }
}