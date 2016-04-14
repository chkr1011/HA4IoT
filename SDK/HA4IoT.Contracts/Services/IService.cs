using Windows.Data.Json;

namespace HA4IoT.Contracts.Services
{
    public interface IService
    {
        JsonObject ExportStatusToJsonObject();
    }
}
