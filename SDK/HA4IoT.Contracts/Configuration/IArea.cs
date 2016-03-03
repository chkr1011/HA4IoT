using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Configuration
{
    public interface IArea : IAutomationController, IActuatorController
    {
        AreaId Id { get; }

        IAreaSettings Settings { get; }

        IController Controller { get; }

        JsonObject ExportConfigurationToJsonObject();

        void LoadSettings();

        void ExposeToApi();
    }
}
