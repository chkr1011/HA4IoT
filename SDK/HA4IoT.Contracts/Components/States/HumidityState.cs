using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.States
{
    public class HumidityState : IComponentFeatureState
    {
        public float? Value { get; set; }

        public JToken Serialize()
        {
            return JToken.FromObject(Value);
        }
    }
}
