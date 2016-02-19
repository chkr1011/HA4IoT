using Windows.Data.Json;

namespace HA4IoT.Networking
{
    public interface IExportToJsonValue
    {
        IJsonValue ExportToJsonObject();
    }
}
