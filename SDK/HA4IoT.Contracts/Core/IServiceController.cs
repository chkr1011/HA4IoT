using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public interface IServiceLocator
    {
        void RegisterService(Type interfaceType, IService service);

        TService GetService<TService>() where TService : IService;

        IList<ServiceRegistration> GetServices();

        bool TryGetService<TService>(out TService service) where TService : IService;
    }
}
