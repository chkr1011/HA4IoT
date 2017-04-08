using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.Features
{
    public class PositionTrackingFeature : IComponentFeature
    {
        public int MaxPosition { get; set; }

        public JToken Serialize()
        {
            return JObject.FromObject(this);
        }
    }
}
