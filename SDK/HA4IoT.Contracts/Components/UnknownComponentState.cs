using Windows.Data.Json;

namespace HA4IoT.Contracts.Components
{
    public class UnknownComponentState : IComponentState
    {
        public IJsonValue ToJsonValue()
        {
            return JsonValue.CreateNullValue();
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
