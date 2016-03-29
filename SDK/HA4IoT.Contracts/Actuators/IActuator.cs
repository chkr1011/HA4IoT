using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Actuators
{
    public interface IActuator
    {
        ActuatorId Id { get; }

        ISettingsContainer Settings { get; }

        IActuatorSettingsWrapper GeneralSettingsWrapper { get; }

        ////void SetActiveState(StateMachineStateId id, params IHardwareParameter[] parameters);

        ////StateMachineStateId GetActiveState(StateMachineStateId id);

        JsonObject ExportConfigurationToJsonObject();

        JsonObject ExportStatusToJsonObject();

        void ExposeToApi(IApiController apiController);
    }
}
