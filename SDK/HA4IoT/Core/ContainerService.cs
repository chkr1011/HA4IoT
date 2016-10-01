using System;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using SimpleInjector;

namespace HA4IoT.Core
{
    public class ContainerService : ServiceBase, IContainerService
    {
        private readonly Container _container;

        public ContainerService(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            _container = container;
        }

        public TService GetInstance<TService>() where TService : class
        {
            return _container.GetInstance<TService>();
        }

        public void RegisterSingleton<TConcrete>() where TConcrete : class
        {
            _container.RegisterSingleton<TConcrete>();
        }

        public void RegisterSingleton<TService>(Func<TService> instanceCreator) where TService : class
        {
            if (instanceCreator == null) throw new ArgumentNullException(nameof(instanceCreator));

            _container.RegisterSingleton<TService>(instanceCreator);
        }

        public void RegisterSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService
        {
            _container.RegisterSingleton<TService, TImplementation>();
        }
    }
}
