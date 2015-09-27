using System;
using System.Collections.Generic;
using CK.HomeAutomation.Core.Timer;

namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class RemoteSwitchController : IOutputController
    {
        private readonly Dictionary<int, RemoteSwitchOutputPort> _ports = new Dictionary<int, RemoteSwitchOutputPort>();
        private readonly LPD433MhzSignalSender _sender;
        private readonly object _syncRoot = new object();
        private readonly IHomeAutomationTimer _timer;

        public RemoteSwitchController(LPD433MhzSignalSender sender, IHomeAutomationTimer timer)
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

        public void Register(int id, LPD433MhzCodeSequence onCodeSequence, LPD433MhzCodeSequence offCodeSequence)
        {
            if (onCodeSequence == null) throw new ArgumentNullException(nameof(onCodeSequence));
            if (offCodeSequence == null) throw new ArgumentNullException(nameof(offCodeSequence));

            lock (_syncRoot)
            {
                var port = new RemoteSwitchOutputPort(id, onCodeSequence, offCodeSequence, _sender, _timer);
                port.Write(BinaryState.Low);

                _ports.Add(id, port);
            }
        }
    }
}
