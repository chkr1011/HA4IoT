using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Components
{
    public class ComponentSettingsWrapper : IActuatorSettingsWrapper
    {
        public const string IsEnabledName = "IsEnabled";

        public ComponentSettingsWrapper(ISettingsContainer settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            Settings = settings;

            IsEnabled = true;
            Settings.SetValue("appSettings", new JsonObject());
        }

        public ISettingsContainer Settings { get; }

        public bool IsEnabled
        {
            get { return Settings.GetBoolean(IsEnabledName); }
            set { Settings.SetValue(IsEnabledName, value); }
        }
    }
}
