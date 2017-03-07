using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class RemoteSocketService : ServiceBase
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, RemoteSocketOutputPort> _ports = new Dictionary<string, RemoteSocketOutputPort>();
        private readonly ILogger _log;

        public RemoteSocketService(ISchedulerService schedulerService, ILogService logService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            // Ensure that the state of the remote switch is restored if the original remote is used
            // or the switch has been removed from the socket and plugged in at another place.
            schedulerService.RegisterSchedule("RCSocketStateSender", TimeSpan.FromSeconds(5), RefreshStates);

            _log = logService.CreatePublisher(nameof(RemoteSocketService));
        }
        
        public ILdp433MhzAdapter Adapter { get; set; }

        public RemoteSocketOutputPort RegisterRemoteSocket(string id, Lpd433MhzCodeSequencePair codeSequencePair)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (codeSequencePair == null) throw new ArgumentNullException(nameof(codeSequencePair));

            lock (_syncRoot)
            {
                var port = new RemoteSocketOutputPort(codeSequencePair, this);
                _ports.Add(id, port);
                port.Write(BinaryState.Low);

                _log.Info($"Registered remote socket '{id}'.");
                return port;
            }
        }

        public RemoteSocketOutputPort GetRemoteSocket(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            
            lock (_syncRoot)
            {
                RemoteSocketOutputPort output;
                if (!_ports.TryGetValue(id, out output))
                {
                    throw new InvalidOperationException($"No remote switch with ID '{id}' registered.");    
                }

                return output;
            }
        }

        public void SendCodeSequence(Lpd433MhzCodeSequence codeSequence)
        {
            if (codeSequence == null) throw new ArgumentNullException(nameof(codeSequence));
            if (Adapter == null)
            {
                _log.Warning("Cannot send code sequence because adapter is not specified.");
                return;
            }

            Adapter.SendCodeSequence(codeSequence);
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
