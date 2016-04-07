using Windows.Data.Json;

namespace HA4IoT.Contracts.Sensors
{
    public interface ISensorValue
    {
        IJsonValue ToJsonValue();
    }
}
