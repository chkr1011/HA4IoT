using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Core.Settings;

namespace HA4IoT.Actuators
{
    public class ActuatorSettingsApiDispatcher : SettingsContainerApiDispatcher<IActuatorSettings>
    {
        public ActuatorSettingsApiDispatcher(IActuatorSettings settings, IApiController apiController)
            : base(settings, $"actuator/{settings.ActuatorId}", apiController)
        {
        }
    }
}
