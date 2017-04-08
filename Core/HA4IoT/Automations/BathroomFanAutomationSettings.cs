using System;
using HA4IoT.Contracts.Automations;

namespace HA4IoT.Automations
{
    public class BathroomFanAutomationSettings : AutomationSettings
    {
        public TimeSpan SlowDuration { get; set; } = TimeSpan.FromMinutes(8);
        public TimeSpan FastDuration { get; set; } = TimeSpan.FromMinutes(12);
    }
}
