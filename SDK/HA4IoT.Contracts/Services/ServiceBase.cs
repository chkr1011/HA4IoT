using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;

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

        public virtual void CompleteRegistration(IServiceController serviceController)
        {
        }
    }
}
