using Windows.Data.Json;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Contracts.Services
{
    public interface IService
    {
        JsonObject ExportStatusToJsonObject();

        void HandleApiCommand(IApiContext apiContext);

        void HandleApiRequest(IApiContext apiContext);
    }
}
