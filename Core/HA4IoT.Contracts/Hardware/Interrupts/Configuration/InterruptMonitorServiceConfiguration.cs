using System.Collections.Generic;

namespace HA4IoT.Contracts.Hardware.Interrupts.Configuration
{
    public class InterruptMonitorServiceConfiguration
    {
        public Dictionary<string, InterruptConfiguration> Interrupts { get; set; } = new Dictionary<string, InterruptConfiguration>();
    }
}
