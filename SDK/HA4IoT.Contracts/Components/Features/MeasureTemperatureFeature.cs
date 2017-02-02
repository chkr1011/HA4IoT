using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.Features
{
    public class MeasureTemperatureFeature : IComponentFeature
    {
        public JToken Serialize()
        {
            return JToken.FromObject(null);
        }
    }
}
