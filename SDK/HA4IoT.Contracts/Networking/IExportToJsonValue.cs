using Windows.Data.Json;

namespace HA4IoT.Contracts.Networking
{
    public interface IExportToJsonValue
    {
        IJsonValue ExportToJsonObject();
    }
}
