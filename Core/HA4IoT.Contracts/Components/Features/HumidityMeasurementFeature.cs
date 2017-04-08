using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.Features
{
    public class HumidityMeasurementFeature : IComponentFeature
    {
        public JToken Serialize()
        {
            return JToken.FromObject(this);
        }
    }
}
