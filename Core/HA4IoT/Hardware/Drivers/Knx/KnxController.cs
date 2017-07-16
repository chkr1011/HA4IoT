using System;
using Windows.Networking;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.Drivers.Knx
{
    public class KnxController
    {
        private readonly object _syncRoot = new object();
        private readonly HostName _hostName;
        private readonly int _port;
        private readonly string _password;

        public KnxController(HostName hostName, int port, string password = "")
        {
            if (hostName == null) throw new ArgumentNullException(nameof(hostName));

            _hostName = hostName;
            _port = port;
            _password = password;
        }

        public KnxDigitalJoinEnpoint CreateDigitalJoinEndpoint(string identifier)
        {
            return new KnxDigitalJoinEnpoint(identifier, this);
        }

        private void Initialization()
        {
            using (var knxClient = new KnxClient(_hostName, _port, _password))
            {
                knxClient.Connect();
                string response = knxClient.SendRequestAndWaitForResponse("i=1");

                Log.Default.Verbose("knx-init-answer: " + response);
            }
        }

        public void SendDigitalJoinOn(string identifier)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));

            lock (_syncRoot)
            {
                using (var knxClient = new KnxClient(_hostName, _port, _password))
                {
                    knxClient.Connect();
                    string response = knxClient.SendRequestAndWaitForResponse(identifier + "=1");

                    Log.Default.Verbose("KnxClient: send-digitalJoinOn: " + response);
                }
            }
        }

        public void SendDigitalJoinOff(string identifier)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));

            lock (_syncRoot)
            {
                using (var knxClient = new KnxClient(_hostName, _port, _password))
                {
                    knxClient.Connect();
                    string response = knxClient.SendRequestAndWaitForResponse(identifier + "=0");

                    Log.Default.Verbose("KnxClient: send-digitalJoinOff: " + response);
                }
            }
        }

        public void SendAnalogJoin(string identifier, double value)
        {
            ////if (identifier == null) throw new ArgumentNullException(nameof(identifier));

            ////if (value < 0)
            ////    value = 0;

            ////if (value > 65535)
            ////    value = 65535;
            
            ////string result = _socketClient.Send(identifier + "=" + value + "\x03");
            ////Log.Verbose("knx-send-analogJoin: " + result);
        }

        public void SendSerialJoin(string identifier, string value)
        {
            ////if (identifier == null) throw new ArgumentNullException(nameof(identifier));

            ////string result = _socketClient.Send(identifier + "=" + value + "\x03");
            ////Log.Verbose("knx-send-SerialJoin: " + result);
        }
    }
}
