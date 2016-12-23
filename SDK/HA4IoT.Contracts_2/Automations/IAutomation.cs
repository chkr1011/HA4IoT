using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Automations
{
    public interface IAutomation
    {
        AutomationId Id { get; }

        JObject ExportStatusToJsonObject();
    }
}
