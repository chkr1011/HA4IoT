using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.Features
{
    public class VerticalMovingFeature : IComponentFeature
    {
        public JToken Serialize()
        {
            return JObject.FromObject(this);
        }
    }
}
