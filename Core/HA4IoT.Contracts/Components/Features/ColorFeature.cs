using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.Features
{
    public class ColorFeature : IComponentFeature
    {
        public JToken Serialize()
        {
            return JObject.FromObject(this);
        }
    }
}
