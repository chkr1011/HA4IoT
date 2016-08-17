using Windows.Data.Json;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Contracts.Services
{
    public abstract class ServiceBase : IApiExposedService
    {
        public virtual void Startup()
        {
        }

        public virtual void HandleApiCall(IApiContext apiContext)
        {
        }

        public virtual JsonObject GetStatus()
        {
            return new JsonObject();
        }
    }
}
