using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Automations
{
    public class TurnOnAndOffAutomationSettings : AutomationSettings
    {
        public TurnOnAndOffAutomationSettings(AutomationId automationId, IHttpRequestController httpApiController, ILogger logger)
            : base(automationId, httpApiController, logger)
        {
        }
    }
}
