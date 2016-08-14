using System.Threading.Tasks;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Contracts.Core
{
    public interface IHA4IoTInitializer
    {
        void RegisterServices(IContainerService containerService);

        Task Configure(IContainerService containerService);
    }
}
