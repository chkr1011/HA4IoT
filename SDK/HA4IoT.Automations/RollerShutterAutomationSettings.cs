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
            DoNotOpenBeforeIsEnabled = new Setting<bool>(false);
            DoNotOpenBeforeTime = new Setting<TimeSpan>(TimeSpan.Parse("07:15"));

            AutoCloseIfTooHotIsEnabled = new Setting<bool>(false);
            AutoCloseIfTooHotTemperaure = new Setting<float>(25);

            DoNotOpenIfTooColdIsEnabled = new Setting<bool>(false);
            DoNotOpenIfTooColdTemperature = new Setting<float>(2);

            OpenOnSunriseOffset = new Setting<TimeSpan>(TimeSpan.FromMinutes(-30));
            CloseOnSunsetOffset = new Setting<TimeSpan>(TimeSpan.FromMinutes(30));
        }

        public Setting<bool> DoNotOpenBeforeIsEnabled { get; private set; } 

        public Setting<TimeSpan> DoNotOpenBeforeTime { get; private set; }

        public Setting<bool> AutoCloseIfTooHotIsEnabled { get; private set; } 

        public Setting<float> AutoCloseIfTooHotTemperaure { get; private set; }

        public Setting<bool> DoNotOpenIfTooColdIsEnabled { get; private set; }

        public Setting<float> DoNotOpenIfTooColdTemperature { get; private set; }

        public Setting<TimeSpan> OpenOnSunriseOffset { get; private set; } 

        public Setting<TimeSpan> CloseOnSunsetOffset { get; private set; } 
    }
}
