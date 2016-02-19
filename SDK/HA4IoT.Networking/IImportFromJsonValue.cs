using Windows.Data.Json;

namespace HA4IoT.Networking
{
    public interface IImportFromJsonValue
    {
        void ImportFromJsonValue(IJsonValue value);
    }
}
