using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Core.Settings;

namespace HA4IoT.Automations
{
    public class AutomationSettingsApiDispatcher : SettingsContainerApiDispatcher<IAutomationSettings>
    {
        public AutomationSettingsApiDispatcher(IAutomationSettings settings, IApiController apiController)
            : base(settings, $"automation/{settings.AutomationId}", apiController)
        {
        }
    }
}
