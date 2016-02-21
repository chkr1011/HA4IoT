using System;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Core.Settings;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomationSettings : AutomationSettings
    {
        public RollerShutterAutomationSettings(AutomationId automationId, IHttpRequestController httpApiController, ILogger logger) 
            : base(automationId, httpApiController, logger)
        {
            DoNotOpenBefore = new Setting<TimeSpan?>(null);
            MinOutsideTemperatureForDoNotOpen = new Setting<float?>(null);
            MaxOutsideTemperatureForAutoClose = new Setting<float?>(null);
        }

        public Setting<TimeSpan?> DoNotOpenBefore { get; private set; }

        public Setting<float?> MaxOutsideTemperatureForAutoClose { get; private set; }

        public Setting<float?> MinOutsideTemperatureForDoNotOpen { get; private set; }
    }
}
