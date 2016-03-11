using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Networking;
using HA4IoT.Core.Settings;

namespace HA4IoT.Automations
{
    public class AutomationSettingsHttpApiDispatcher : SettingsContainerHttpApiDispatcher<IAutomationSettings>
    {
        public AutomationSettingsHttpApiDispatcher(IAutomationSettings settings, IApiController apiController)
            : base(settings, $"automation/{settings.AutomationId}", apiController)
        {
        }
    }
}
