using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Actuators
{
    public class ActuatorSettingsWrapper : IActuatorSettingsWrapper
    {
        public const string IsEnabledName = "IsEnabled";

        public ActuatorSettingsWrapper(ISettingsContainer settings)
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
