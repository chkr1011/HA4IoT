using System;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomationSettings : AutomationSettings
    {
        public bool SkipBeforeTimestampIsEnabled { get; set; }

        public TimeSpan SkipBeforeTimestamp { get; set; } = TimeSpan.Parse("07:15");

        public bool AutoCloseIfTooHotIsEnabled { get; set; }

        public float AutoCloseIfTooHotTemperaure { get; set; } = 25;

        public bool SkipIfFrozenIsEnabled { get; set; }

        public float SkipIfFrozenTemperature { get; set; } = 2;

        public bool SkipNextOpenOnSunrise { get; set; }

        public bool SkipNextCloseOnSunset { get; set; }

        public TimeSpan OpenOnSunriseOffset { get; set; } = TimeSpan.FromMinutes(-30);

        public TimeSpan CloseOnSunsetOffset { get; set; } = TimeSpan.FromMinutes(30);
    }
}
