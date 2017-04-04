using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public interface IComponentFeature
    {
        JToken Serialize();
    }
}
