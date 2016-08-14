using Windows.Data.Json;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Areas
{
    public interface IArea
    {
        AreaId Id { get; }

        ISettingsContainer Settings { get; }
        
        JsonObject ExportConfigurationToJsonObject();

        void AddComponent(IComponent component);

        TComponent GetComponent<TComponent>() where TComponent : IComponent;

        TComponent GetComponent<TComponent>(ComponentId id) where TComponent : IComponent;
    }
}
