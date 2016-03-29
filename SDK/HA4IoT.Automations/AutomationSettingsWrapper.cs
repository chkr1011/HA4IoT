using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Automations
{
    public class AutomationSettingsWrapper : IAutomationSettingsWrapper
    {
        public const string IsEnabledName = "IsEnabled";

        public AutomationSettingsWrapper(ISettingsContainer settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            Settings = settings;

            IsEnabled = true;
            Settings.SetValue("AppSettings", new JsonObject());
        }

        public ISettingsContainer Settings { get; }

        public bool IsEnabled
        {
            get { return Settings.GetBoolean(IsEnabledName); }
            set { Settings.SetValue(IsEnabledName, value); }
        }
    }
}
