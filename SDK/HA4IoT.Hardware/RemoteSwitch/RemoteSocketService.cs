using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class RemoteSocketService : ServiceBase, IBinaryOutputController
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, RemoteSocketOutputPort> _ports = new Dictionary<int, RemoteSocketOutputPort>();
        
        public RemoteSocketService(ISchedulerService schedulerService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            
            // Ensure that the state of the remote switch is restored if the original remote is used
            // or the switch has been removed from the socket and plugged in at another place.
            schedulerService.RegisterSchedule("RCSocketStateSender", TimeSpan.FromSeconds(5), RefreshStates);
        }
        
        public LPD433MHzSignalSender Sender { get; set; }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0) throw new ArgumentOutOfRangeException(nameof(number));

            lock (_syncRoot)
            {
                RemoteSocketOutputPort output;
                if (!_ports.TryGetValue(number, out output))
                {
                    throw new InvalidOperationException("No remote switch with ID " + number + " is registered.");    
                }

                return output;
            }
        }

        public RemoteSocketOutputPort RegisterRemoteSocket(int id, LPD433MHzCodeSequencePair codeSequencePair)
        {
            if (codeSequencePair == null) throw new ArgumentNullException(nameof(codeSequencePair));

            if (Sender == null)
            {
                throw new InvalidOperationException("No sender is set for remote sockets. Registration not possible.");
            }

            lock (_syncRoot)
            {
                var port = new RemoteSocketOutputPort(codeSequencePair, Sender);
                port.Write(BinaryState.Low);

                _ports.Add(id, port);

                return port;
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
