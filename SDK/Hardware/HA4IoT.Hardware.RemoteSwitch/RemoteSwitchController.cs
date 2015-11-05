using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core.Timer;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class RemoteSwitchController : IBinaryOutputController
    {
        private readonly object _syncRoot = new object();

        private readonly Dictionary<int, RemoteSwitchOutputPort> _ports = new Dictionary<int, RemoteSwitchOutputPort>();
        private readonly LPD433MHzSignalSender _sender;

        public RemoteSwitchController(LPD433MHzSignalSender sender, IHomeAutomationTimer timer)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _sender = sender;

            // Ensure that the state of the remote switch is restored if the original remote is used
            // or the switch has been removed from the socket and plugged in at another place.
            timer.Every(TimeSpan.FromSeconds(5)).Do(RefreshStates);
        }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0) throw new ArgumentOutOfRangeException(nameof(number));

            lock (_syncRoot)
            {
                RemoteSwitchOutputPort output;
                if (!_ports.TryGetValue(number, out output))
                {
                    throw new InvalidOperationException("No remote switch with ID " + number + " is registered.");    
                }

                return output;
            }
        }

        public void Register(int id, LPD433MHzCodeSequence onCodeSequence, LPD433MHzCodeSequence offCodeSequence)
        {
            if (onCodeSequence == null) throw new ArgumentNullException(nameof(onCodeSequence));
            if (offCodeSequence == null) throw new ArgumentNullException(nameof(offCodeSequence));

            lock (_syncRoot)
            {
                var port = new RemoteSwitchOutputPort(id, onCodeSequence, offCodeSequence, _sender);
                port.Write(BinaryState.Low);

                _ports.Add(id, port);
            }
        }

        private void RefreshStates()
        {
            foreach (var port in _ports.Values)
            {
                port.Write(port.Read());
            }
        }
    }
}
