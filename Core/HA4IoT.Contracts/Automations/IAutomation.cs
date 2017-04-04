using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Automations
{
    public interface IAutomation
    {
        string Id { get; }

        JObject ExportStatusToJsonObject();
    }
}
