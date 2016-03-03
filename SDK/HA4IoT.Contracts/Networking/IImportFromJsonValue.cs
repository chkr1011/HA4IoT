using Windows.Data.Json;

namespace HA4IoT.Contracts.Networking
{
    public interface IImportFromJsonValue
    {
        void ImportFromJsonValue(IJsonValue value);
    }
}
