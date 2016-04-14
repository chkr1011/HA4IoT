using Windows.Data.Json;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Areas
{
    public interface IArea : IAutomationController, IComponentController
    {
        AreaId Id { get; }

        ISettingsContainer Settings { get; }

        IController Controller { get; }

        JsonObject ExportConfigurationToJsonObject();
    }
}
