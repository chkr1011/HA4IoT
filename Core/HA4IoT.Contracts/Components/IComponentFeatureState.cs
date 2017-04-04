using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public interface IComponentFeatureState
    {
        JToken Serialize();
    }
}
