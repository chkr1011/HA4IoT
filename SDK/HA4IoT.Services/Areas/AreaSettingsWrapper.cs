using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Services.Areas
{
    public class AreaSettingsWrapper : IAreaSettingsWrapper
    {
        public AreaSettingsWrapper(ISettingsContainer settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            Settings = settings;
            Settings.SetValue("appSettings", new JsonObject());
        }

        public ISettingsContainer Settings { get; }
    }
}
