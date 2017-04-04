using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.Features
{
    public class ButtonFeature : IComponentFeature
    {
        public JToken Serialize()
        {
            return JObject.FromObject(this);
        }
    }
}
