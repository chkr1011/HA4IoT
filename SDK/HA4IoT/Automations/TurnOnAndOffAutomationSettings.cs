using System;

namespace HA4IoT.Automations
{
    public class TurnOnAndOffAutomationSettings : AutomationSettings
    {
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(60);
    }
}
