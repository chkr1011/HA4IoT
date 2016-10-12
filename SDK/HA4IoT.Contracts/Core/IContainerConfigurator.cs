using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Contracts.Core
{
    public interface IContainerConfigurator
    {
        void ConfigureContainer(IContainer container);
    }
}
