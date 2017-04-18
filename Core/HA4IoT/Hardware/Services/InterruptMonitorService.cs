using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.Services
{
    public class InterruptMonitorService : ServiceBase
    {
        private readonly Dictionary<string, InterruptMonitor> _interruptMonitors = new Dictionary<string, InterruptMonitor>();
        private readonly ILogService _logService;

        public InterruptMonitorService(ILogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public void RegisterInterrupt(string id, IBinaryInput input)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var interruptMonitor = new InterruptMonitor(id, input, _logService);
            lock (_interruptMonitors)
            {
                _interruptMonitors.Add(id, interruptMonitor);
            }
        }

        public void RegisterCallback(string interruptMonitorId, Action callback)
        {
            if (interruptMonitorId == null) throw new ArgumentNullException(nameof(interruptMonitorId));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            lock (_interruptMonitors)
            {
                _interruptMonitors[interruptMonitorId].AddCallback(callback);
            }
        }
    }
}
