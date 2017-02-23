using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.States
{
    public class StateMachineFeatureState : IComponentFeatureState
    {
        public StateMachineFeatureState(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public JToken Serialize()
        {
            return JToken.FromObject(Value);
        }
    }
}
