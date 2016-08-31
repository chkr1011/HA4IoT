using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public class UnknownComponentState : IComponentState
    {
        private readonly JToken _jsonValue;

        public UnknownComponentState()
        {
            _jsonValue = JValue.CreateNull();
        }

        public JToken ToJsonValue()
        {
            return _jsonValue;
        }

        public bool Equals(IComponentState otherState)
        {
            return otherState is UnknownComponentState;
        }

        public override string ToString()
        {
            return "<Unknown>";
        }
    }
}
