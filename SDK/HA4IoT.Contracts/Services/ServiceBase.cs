using Windows.Data.Json;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Contracts.Services
{
    public abstract class ServiceBase : IService
    {
        public virtual JsonObject ExportStatusToJsonObject()
        {
            return new JsonObject();
        }

        public virtual void HandleApiCommand(IApiContext apiContext)
        {
        }

        public virtual void HandleApiRequest(IApiContext apiContext)
        {
            apiContext.Response = ExportStatusToJsonObject();
        }
    }
}
