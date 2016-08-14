using System.Threading.Tasks;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Contracts.Core
{
    public interface IInitializer
    {
        void RegisterServices(IContainerService containerService);

        Task Initialize(IContainerService containerService);
    }
}
