using Windows.Data.Json;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Areas
{
    public interface IArea : IComponentService, IAutomationService
    {
        AreaId Id { get; }

        ISettingsContainer Settings { get; }
        
        JsonObject ExportConfigurationToJsonObject();
    }
}
