using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Components
{
    public interface IComponent
    {
        ComponentId Id { get; }

        ISettingsContainer Settings { get; }

        IActuatorSettingsWrapper GeneralSettingsWrapper { get; }
        // TODO: Consider move to separate class (Composition over Inheritance)
        JsonObject ExportConfigurationToJsonObject();

        JsonObject ExportStatusToJsonObject();

        void ExposeToApi(IApiController apiController);
    }
}
