using HA4IoT.Contracts.Core;

namespace HA4IoT.Core
{
    public class ControllerOptions
    {
        public int? StatusLedNumber { get; set; }

        public IConfigurator Configurator { get; set; }
    }
}
