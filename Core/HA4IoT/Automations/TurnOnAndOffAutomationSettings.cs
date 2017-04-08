using System;
using HA4IoT.Contracts.Automations;

namespace HA4IoT.Automations
{
    public class TurnOnAndOffAutomationSettings : AutomationSettings
    {
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(60);
    }
}
