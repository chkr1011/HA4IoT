using Windows.Data.Json;

namespace HA4IoT.Networking
{
    public interface IConvertibleToJsonValue
    {
        IJsonValue ToJsonValue();
    }
}
