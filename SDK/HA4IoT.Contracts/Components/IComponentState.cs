using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public interface IComponentState
    {
        JToken ToJsonValue();

        bool Equals(IComponentState otherState);

        string ToString();
    }
}
