using System.Collections;
using System.Collections.Generic;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public interface IServiceController
    {
        void RegisterService<TService>(TService service) where TService : IService;

        TService GetService<TService>() where TService : IService;

        IList<IService> GetServices();

        bool TryGetService<TService>(out TService service) where TService : IService;
    }
}
