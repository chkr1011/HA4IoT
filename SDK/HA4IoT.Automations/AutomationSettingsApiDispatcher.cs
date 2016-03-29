using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Core.Settings;

namespace HA4IoT.Automations
{
    public class AutomationSettingsApiDispatcher : SettingsContainerApiDispatcher
    {
        public AutomationSettingsApiDispatcher(IAutomation automation, IApiController apiController)
            : base(automation.Settings, $"automation/{automation.Id}", apiController)
        {
        }
    }
}
