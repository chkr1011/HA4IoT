using System;
using System.Collections.Generic;
using CK.HomeAutomation.Core.Timer;

namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class RemoteSwitchController : IOutputController
    {
        private readonly Dictionary<int, RemoteSwitchOutputPort> _ports = new Dictionary<int, RemoteSwitchOutputPort>();
        private readonly RemoteSwitchSender _sender;
        private readonly object _syncRoot = new object();
        private readonly IHomeAutomationTimer _timer;

        public RemoteSwitchController(RemoteSwitchSender sender, IHomeAutomationTimer timer)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _sender = sender;
            _timer = timer;
        }

        public IBinaryOutput GetOutput(int number)
        {
            lock (_syncRoot)
            {
                return _ports[number];
            }
        }

        public void Register(int id, RemoteSwitchCode onCode, RemoteSwitchCode offCode)
        {
            lock (_syncRoot)
            {
                var port = new RemoteSwitchOutputPort(id, onCode, offCode, _sender, _timer);
                port.Write(BinaryState.Low);

                _ports.Add(id, port);
            }
        }
    }
}
