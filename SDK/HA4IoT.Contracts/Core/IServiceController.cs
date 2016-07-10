using System.Collections.Generic;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public interface IServiceLocator
    {
        void RegisterService(IService service);

        TService GetService<TService>() where TService : IService;

        IList<IService> GetServices();

        bool TryGetService<TService>(out TService service) where TService : IService;
    }
}
