using Windows.Data.Json;

namespace HA4IoT.Contracts.Components
{
    public interface IComponentState
    {
        IJsonValue ToJsonValue();

        bool Equals(IComponentState otherState);

        string ToString();
    }
}
