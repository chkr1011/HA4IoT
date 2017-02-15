using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.Features
{
    public class LevelStateFeature : IComponentFeature
    {
        public int MaxLevel { get; set; }

        public JToken Serialize()
        {
            return JObject.FromObject(this);
        }
    }
}
