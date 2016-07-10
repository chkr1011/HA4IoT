using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Services
{
    public interface IService
    {
        JsonObject ExportStatusToJsonObject();

        void HandleApiCommand(IApiContext apiContext);

        void HandleApiRequest(IApiContext apiContext);

        void CompleteRegistration(IServiceLocator serviceLocator);
    }
}
