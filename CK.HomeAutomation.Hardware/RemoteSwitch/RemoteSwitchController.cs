using System;
using System.Collections.Generic;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware.Drivers;

namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class RemoteSwitchController : IOutputController
    {
        private readonly object _syncRoot = new object();
        private readonly RemoteSwitchBridgeDriver _bridge;
        private readonly HomeAutomationTimer _timer;
        private readonly Dictionary<int, RemoteSwitchPort> _ports = new Dictionary<int, RemoteSwitchPort>();
        
        public RemoteSwitchController(RemoteSwitchBridgeDriver bridge, HomeAutomationTimer timer)
        {
            if (bridge == null) throw new ArgumentNullException(nameof(bridge));
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _bridge = bridge;
            _timer = timer;
        }

        public void Register(int id, RemoteSwitchCode onCode, RemoteSwitchCode offCode)
        {
            lock (_syncRoot)
            {
                var port = new RemoteSwitchPort(id, onCode, offCode, _bridge, _timer);
                port.Write(BinaryState.Low);

                _ports.Add(id, port);
            }
        }

        public IBinaryOutput GetOutput(int number)
        {
            lock (_syncRoot)
            {
                return _ports[number];
            }
        }
    }
}
