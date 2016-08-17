using Windows.Data.Json;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Contracts.Services
{
    public interface IApiExposedService : IService
    {
        void HandleApiCall(IApiContext apiContext);

        JsonObject GetStatus();
    }
}
