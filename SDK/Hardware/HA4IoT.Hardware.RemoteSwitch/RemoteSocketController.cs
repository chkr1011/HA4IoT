using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class RemoteSocketController : IBinaryOutputController
    {
        private readonly object _syncRoot = new object();

        private readonly Dictionary<int, RemoteSocketOutputPort> _ports = new Dictionary<int, RemoteSocketOutputPort>();
        private readonly LPD433MHzSignalSender _sender;

        public RemoteSocketController(LPD433MHzSignalSender sender, ISchedulerService schedulerService)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));

            _sender = sender;

            // Ensure that the state of the remote switch is restored if the original remote is used
            // or the switch has been removed from the socket and plugged in at another place.
            schedulerService.RegisterSchedule("RCSocketStateSender", TimeSpan.FromSeconds(5), RefreshStates);
        }

        public DeviceId Id { get; } = new DeviceId("RemoteSocketController");

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

        public RemoteSocketController WithRemoteSocket(int id, LPD433MHzCodeSequencePair codeSequencePair)
        {
            if (codeSequencePair == null) throw new ArgumentNullException(nameof(codeSequencePair));

            lock (_syncRoot)
            {
                var port = new RemoteSocketOutputPort(id, codeSequencePair, _sender);
                port.Write(BinaryState.Low);

                _ports.Add(id, port);
            }

            return this;
        }

        public void HandleApiCommand(IApiContext apiContext)
        {
        }

        public void HandleApiRequest(IApiContext apiContext)
        {
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
