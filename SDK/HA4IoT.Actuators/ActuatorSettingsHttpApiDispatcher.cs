using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Networking;
using HA4IoT.Core.Settings;

namespace HA4IoT.Actuators
{
    public class ActuatorSettingsHttpApiDispatcher : SettingsContainerHttpApiDispatcher<IActuatorSettings>
    {
        public ActuatorSettingsHttpApiDispatcher(IActuatorSettings settings, IHttpRequestController httpApiController)
            : base(settings, $"actuator/{settings.ActuatorId}", httpApiController)
        {
        }
    }
}
