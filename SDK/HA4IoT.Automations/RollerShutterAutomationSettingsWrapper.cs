using System;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomationSettingsWrapper : AutomationSettingsWrapper
    {
        public RollerShutterAutomationSettingsWrapper(ISettingsContainer settings) 
            : base(settings)
        {
            SkipBeforeTimestampIsEnabled = false;
            SkipBeforeTimestamp = TimeSpan.Parse("07:15");

            AutoCloseIfTooHotIsEnabled = false;
            AutoCloseIfTooHotTemperaure = 25;

            SkipIfFrozenIsEnabled = false;
            SkipIfFrozenTemperature = 2;

            OpenOnSunriseOffset = TimeSpan.FromMinutes(-30);
            CloseOnSunsetOffset = TimeSpan.FromMinutes(30);
        }

        public bool SkipBeforeTimestampIsEnabled
        {
            get { return Settings.GetBoolean("SkipBeforeTimestampIsEnabled"); }
            set { Settings.SetValue("SkipBeforeTimestampIsEnabled", value); }
        }

        public TimeSpan SkipBeforeTimestamp
        {
            get { return Settings.GetTimeSpan("SkipBeforeTimestamp"); }
            set { Settings.SetValue("SkipBeforeTimestamp", value); }
        }

        public bool AutoCloseIfTooHotIsEnabled
        {
            get { return Settings.GetBoolean("AutoCloseIfTooHotIsEnabled"); }
            set { Settings.SetValue("AutoCloseIfTooHotIsEnabled", value); }
        }

        public float AutoCloseIfTooHotTemperaure
        {
            get { return Settings.GetFloat("AutoCloseIfTooHotTemperaure"); }
            set { Settings.SetValue("AutoCloseIfTooHotTemperaure", value); }
        }

        public bool SkipIfFrozenIsEnabled
        {
            get { return Settings.GetBoolean("SkipIfFrozenIsEnabled"); }
            set { Settings.SetValue("SkipIfFrozenIsEnabled", value); }
        }

        public float SkipIfFrozenTemperature
        {
            get { return Settings.GetFloat("SkipIfFrozenTemperature"); }
            set { Settings.SetValue("SkipIfFrozenTemperature", value); }
        }

        public TimeSpan OpenOnSunriseOffset
        {
            get { return Settings.GetTimeSpan("OpenOnSunriseOffset"); }
            set { Settings.SetValue("OpenOnSunriseOffset", value); }
        }

        public TimeSpan CloseOnSunsetOffset
        {
            get { return Settings.GetTimeSpan("OpenOnSunriseOffset"); }
            set { Settings.SetValue("OpenOnSunriseOffset", value); }
        }
    }
}
