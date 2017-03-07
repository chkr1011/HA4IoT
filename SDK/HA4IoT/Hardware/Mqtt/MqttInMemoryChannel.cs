using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace HA4IoT.Hardware.Mqtt
{
    extern alias mqttClient;

    public class MqttInMemoryChannel
    {
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private readonly ConcurrentQueue<byte> _stream = new ConcurrentQueue<byte>();
        
        public bool DataAvailable => _stream.Count > 0;

        public MqttInMemoryChannel Partner { get; set; }

        public int Receive(byte[] buffer)
        {
            return Read(buffer, Timeout.Infinite);
        }

        public int Receive(byte[] buffer, int timeout)
        {
            return Read(buffer, timeout);
        }

        public int Send(byte[] buffer)
        {
            return Partner.WriteInternal(buffer);
        }

        public void Close()
        {
        }

        public void Connect()
        {
        }

        public void Accept()
        {
        }

        private int WriteInternal(byte[] data)
        {
            foreach (var @byte in data)
            {
                _stream.Enqueue(@byte);
            }

            _resetEvent.Set();
            return data.Length;
        }

        private int Read(byte[] buffer, int timeout)
        {
            var stopwatch = Stopwatch.StartNew();

            var i = 0;
            while (i < buffer.Length)
            {
                if (_stream.TryDequeue(out buffer[i]))
                {
                    i++;
                }
                else
                {
                    _resetEvent.Reset();
                    _resetEvent.WaitOne();
                }
                
                if (timeout != Timeout.Infinite && stopwatch.ElapsedMilliseconds > timeout)
                {
                    break;
                }
            }

            return i;
        }
    }
}