using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Core
{
    public class ControllerOptions
    {
        public int HttpServerPort { get; set; } = 80;

        public int? StatusLedGpio { get; set; }

        public Type ConfigurationType { get; set; }

        public IContainerConfigurator ContainerConfigurator { get; set; }
    }
}
