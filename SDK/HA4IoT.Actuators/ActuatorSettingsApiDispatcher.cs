using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Core.Settings;

namespace HA4IoT.Actuators
{
    public class ActuatorSettingsApiDispatcher : SettingsContainerApiDispatcher
    {
        public ActuatorSettingsApiDispatcher(IComponent component, IApiController apiController)
            : base(component.Settings, $"component/{component.Id}", apiController)
        {
        }
    }
}
