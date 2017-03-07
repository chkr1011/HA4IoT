using System;
using uPLibrary.Networking.M2Mqtt;

namespace HA4IoT.Hardware.Mqtt
{
    public class BrokerMqttStream : IMqttNetworkChannel
    {
        public BrokerMqttStream(MqttInMemoryChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            Channel = channel;
        }

        public MqttInMemoryChannel Channel { get; set; }

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