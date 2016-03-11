using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core.Settings;

namespace HA4IoT.Automations
{
    public class TurnOnAndOffAutomationSettings : AutomationSettings
    {
        public TurnOnAndOffAutomationSettings(AutomationId automationId, IApiController apiController, ILogger logger) : base(automationId, apiController, logger)
        {
            Duration = new Setting<TimeSpan>(TimeSpan.FromSeconds(60));
        }

        public Setting<TimeSpan> Duration { get; } 
    }
}
