using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;

namespace HA4IoT.Core
{
    public class AreaSettings : SettingsContainer, IAreaSettings
    {
        public AreaSettings(AreaId areaId) 
            : base(GenerateFilename(areaId))
        {
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));

            AreaId = areaId;

            AppSettings = new Setting<JsonObject>(new JsonObject());
        }
        
        [HideFromToJsonObject]
        public AreaId AreaId { get; }

        public ISetting<JsonObject> AppSettings { get; }

        private static string GenerateFilename(AreaId areaId)
        {
            return StoragePath.WithFilename("Areas", areaId.Value, "Settings.json");
        }
    }
}
