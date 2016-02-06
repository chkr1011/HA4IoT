using Windows.Data.Json;

namespace HA4IoT.Contracts.Core
{
    public interface IStatusProvider
    {
        JsonObject GetStatusForApi();
    }
}
