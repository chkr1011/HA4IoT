using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Sensors
{
    public static class SwitchStateId
    {
        public static readonly JToken Off = JToken.FromObject("Off");
        public static readonly JToken On = JToken.FromObject("On");
    }
}
