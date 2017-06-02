using HA4IoT.Contracts.Hardware.RemoteSockets.Protocols;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HA4IoT.Contracts.Hardware.RemoteSockets.Configuration
{
    public sealed class IntertechnoConfiguration
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public IntertechnoSystemCode SystemCode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public IntertechnoUnitCode UnitCode { get; set; }
    }
}
