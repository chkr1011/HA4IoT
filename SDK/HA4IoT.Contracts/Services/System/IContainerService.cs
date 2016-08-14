using System;
using System.Collections.Generic;
using SimpleInjector;

namespace HA4IoT.Contracts.Services.System
{
    public interface IContainerService
    {
        TService GetInstance<TService>() where TService : class;

        IEnumerable<InstanceProducer> GetCurrentRegistrations();

        void RegisterSingleton<TConcrete>() where TConcrete : class;

        void RegisterSingleton<TService, TImplementation>() where TService : class
            where TImplementation : class, TService;

        void RegisterSingleton<TService>(Func<TService> instanceCreator) where TService : class;
    }
}