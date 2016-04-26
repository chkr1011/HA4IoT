using System;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Automations
{
    public class TurnOnAndOffAutomationSettingsWrapper : AutomationSettingsWrapper
    {
        public TurnOnAndOffAutomationSettingsWrapper(ISettingsContainer settingsContainer) 
            : base(settingsContainer)
        {
            Duration = TimeSpan.FromSeconds(60);
        }

        public TimeSpan Duration
        {
            get { return Settings.GetTimeSpan("Duration"); }
            set { Settings.SetValue("Duration", value); }
        }
    }
}
