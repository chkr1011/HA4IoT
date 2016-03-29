using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Core.Settings;

namespace HA4IoT.Actuators
{
    public class ActuatorSettingsApiDispatcher : SettingsContainerApiDispatcher
    {
        public ActuatorSettingsApiDispatcher(IActuator actuator, IApiController apiController)
            : base(actuator.Settings, $"actuator/{actuator.Id}", apiController)
        {
        }
    }
}
