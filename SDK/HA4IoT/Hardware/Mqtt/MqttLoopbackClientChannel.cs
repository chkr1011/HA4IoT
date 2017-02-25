using System.IO;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;

namespace HA4IoT.Hardware.Mqtt
{
    public class MqttLoopbackClientChannel : IMqttNetworkChannel
    {
        private readonly object _syncRoot = new object();
        private readonly ManualResetEvent _memorySync = new ManualResetEvent(false);
        private readonly MemoryStream _memory = new MemoryStream();

        public bool DataAvailable
        {
            get
            {
                lock (_syncRoot)
                {
                    return _memory.Length > 0;
                }
            }
        }

        public int Receive(byte[] buffer)
        {
            return Receive(buffer, Timeout.Infinite);
        }

        public int Receive(byte[] buffer, int timeout)
        {
            if (!_memorySync.WaitOne(timeout))
            {
                return 0;
            }

            lock (_syncRoot)
            {
                var length = _memory.Read(buffer, 0, buffer.Length);

                if (_memory.Position == _memory.Length)
                {
                    _memory.SetLength(0);
                    _memorySync.Reset();
                }

                return length;
            }
        }

        public int Send(byte[] buffer)
        {
            lock (_syncRoot)
            {
                _memory.Write(buffer, 0, buffer.Length);
                _memory.Position -= buffer.Length;

                _memorySync.Set();
                return buffer.Length;
            }
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
    }
}