using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Core.Settings;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomationSettings : AutomationSettings
    {
        public RollerShutterAutomationSettings(AutomationId automationId, IApiController apiController) 
            : base(automationId, apiController)
        {
            SkipBeforeTimestampIsEnabled = new Setting<bool>(false);
            SkipBeforeTimestamp = new Setting<TimeSpan>(TimeSpan.Parse("07:15"));

            AutoCloseIfTooHotIsEnabled = new Setting<bool>(false);
            AutoCloseIfTooHotTemperaure = new Setting<float>(25);

            SkipIfFrozenIsEnabled = new Setting<bool>(false);
            SkipIfFrozenTemperature = new Setting<float>(2);

            OpenOnSunriseOffset = new Setting<TimeSpan>(TimeSpan.FromMinutes(-30));
            CloseOnSunsetOffset = new Setting<TimeSpan>(TimeSpan.FromMinutes(30));
        }

        public Setting<bool> SkipBeforeTimestampIsEnabled { get; private set; } 

        public Setting<TimeSpan> SkipBeforeTimestamp { get; private set; }

        public Setting<bool> AutoCloseIfTooHotIsEnabled { get; private set; } 

        public Setting<float> AutoCloseIfTooHotTemperaure { get; private set; }

        public Setting<bool> SkipIfFrozenIsEnabled { get; private set; }

        public Setting<float> SkipIfFrozenTemperature { get; private set; }

        public Setting<TimeSpan> OpenOnSunriseOffset { get; private set; } 

        public Setting<TimeSpan> CloseOnSunsetOffset { get; private set; } 
    }
}
