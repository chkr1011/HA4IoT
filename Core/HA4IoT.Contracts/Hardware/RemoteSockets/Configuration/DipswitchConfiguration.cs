using HA4IoT.Contracts.Hardware.RemoteSockets.Protocols;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HA4IoT.Contracts.Hardware.RemoteSockets.Configuration
{
    public sealed class DipswitchConfiguration
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public DipswitchSystemCode SystemCode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DipswitchUnitCode UnitCode { get; set; }
    }
}
