using HA4IoT.Contracts.Core;

namespace HA4IoT.Core
{
    public class ControllerOptions
    {
        public int HttpServerPort { get; set; } = 80;

        public int? StatusLedNumber { get; set; }

        public IConfiguration Configuration { get; set; }
    }
}
