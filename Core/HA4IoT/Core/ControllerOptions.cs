using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Core
{
    public class ControllerOptions
    {
        public Type ConfigurationType { get; set; }

        public IContainerConfigurator ContainerConfigurator { get; set; }

        public ICollection<ILogAdapter> LogAdapters { get; } = new Collection<ILogAdapter>();

        public ICollection<IService> CustomServices { get; } = new Collection<IService>();
    }
}
