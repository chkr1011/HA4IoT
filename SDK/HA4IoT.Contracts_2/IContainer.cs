using System;

namespace HA4IoT.Contracts
{
    public interface IContainer
    {
        TService GetInstance<TService>() where TService : class;
        
        void RegisterSingleton<TConcrete>() where TConcrete : class;

        void RegisterSingleton<TService, TImplementation>() where TService : class
            where TImplementation : class, TService;

        void RegisterSingleton<TService>(Func<TService> instanceCreator) where TService : class;
    }
}