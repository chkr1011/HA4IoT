using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Core
{
    public interface IAreaSettings : ISettingsContainer
    {
        AreaId AreaId { get; }

        ISetting<JsonObject> AppSettings { get; }

        void Load();
    }
}
