using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public static class BinaryStateId
    {
        // TODO: Rename to BinaryState.Off etc. (without id)
        public static readonly NamedComponentState Off = new NamedComponentState("Off");
        public static readonly NamedComponentState On = new NamedComponentState("On");
    }
}
