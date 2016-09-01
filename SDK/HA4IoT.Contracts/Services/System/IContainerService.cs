using System;

namespace HA4IoT.Contracts.Services.System
{
    public interface IContainerService : IService
    {
        TService GetInstance<TService>() where TService : class;
        
        void RegisterSingleton<TConcrete>() where TConcrete : class;

        void RegisterSingleton<TService, TImplementation>() where TService : class
            where TImplementation : class, TService;

        void RegisterSingleton<TService>(Func<TService> instanceCreator) where TService : class;
    }
}