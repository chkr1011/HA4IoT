using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.Interrupts;
using HA4IoT.Contracts.Hardware.Interrupts.Configuration;
using HA4IoT.Contracts.Hardware.RaspberryPi;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.Interrupts
{
    public sealed class InterruptMonitorService : ServiceBase, IInterruptMonitorService
    {
        private readonly Dictionary<string, InterruptMonitor> _interruptMonitors = new Dictionary<string, InterruptMonitor>();

        private readonly IGpioService _gpioService;
        private readonly IConfigurationService _configurationService;
        private readonly ILogService _logService;
        private readonly ILogger _log;

        public InterruptMonitorService(IConfigurationService configurationService, IGpioService gpioService, ILogService logService)
        {
            _gpioService = gpioService ?? throw new ArgumentNullException(nameof(gpioService));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _log = logService.CreatePublisher(nameof(InterruptMonitorService));
        }

        public void RegisterInterrupts()
        {
            var configuration = _configurationService.GetConfiguration<InterruptMonitorServiceConfiguration>("InterruptMonitorService");
            foreach (var interruptConfiguration in configuration.Interrupts)
            {
                var gpio = _gpioService.GetInput(interruptConfiguration.Value.Gpio, interruptConfiguration.Value.PullMode, interruptConfiguration.Value.MonitoringMode);
                if (interruptConfiguration.Value.IsInverted)
                {
                    gpio = gpio.WithInvertedState();
                }

                RegisterInterrupt(interruptConfiguration.Key, gpio);
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

        private void RegisterInterrupt(string id, IBinaryInput input)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var interruptMonitor = new InterruptMonitor(id, input, _logService);
            lock (_interruptMonitors)
            {
                _interruptMonitors.Add(id, interruptMonitor);
            }

            _log.Verbose($"Registered interrupt '{id}'.");
        }
    }
}
