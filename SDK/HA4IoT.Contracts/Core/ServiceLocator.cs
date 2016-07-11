using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public class ServiceLocator : IServiceLocator
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();
        
        public void RegisterService(Type interfaceType, IService service)
        {
            if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));
            if (service == null) throw new ArgumentNullException(nameof(service));

            lock (_syncRoot)
            {
                if (!interfaceType.IsInstanceOfType(service))
                {
                    throw new InvalidOperationException("The service is not implementing the required interface.");
                }

                if (_services.ContainsKey(interfaceType))
                {
                    throw new ServiceAlreadyRegisteredException(interfaceType);
                }

                _services.Add(interfaceType, service);
            }

            service.CompleteRegistration(this);
        }

        public TService GetService<TService>() where TService : IService
        {
            var interfaceType = typeof(TService);

            lock (_syncRoot)
            {
                IService serviceBuffer;
                if (_services.TryGetValue(interfaceType, out serviceBuffer))
                {
                    return (TService) serviceBuffer;
                }
            }

            throw new ServiceNotRegisteredException(typeof(TService));
        }

        public bool TryGetService<TService>(out TService service) where TService : IService
        {
            var interfaceType = typeof(TService);

            lock (_syncRoot)
            {
                IService serviceBuffer;
                if (_services.TryGetValue(interfaceType, out serviceBuffer))
                {
                    service = (TService) serviceBuffer;
                    return true;
                }
            }

            service = default(TService);
            return false;
        }

        public IList<ServiceRegistration> GetServices()
        {
            lock (_syncRoot)
            {
                return _services.Select(s => new ServiceRegistration(s.Key, s.Value)).ToList();
            }
        }
    }
}
