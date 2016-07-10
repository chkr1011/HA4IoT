using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public class ServiceLocator : IServiceLocator
    {
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();
        
        public void RegisterService(IService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var serviceType = service.GetType();
            if (_services.Keys.Any(s => serviceType.IsAssignableFrom(s)))
            {
                throw new ServiceAlreadyRegisteredException(serviceType);
            }

            _services.Add(serviceType, service);
            service.CompleteRegistration(this);
        }

        public TService GetService<TService>() where TService : IService
        {
            var serviceType = typeof(TService);
            var key = _services.Keys.FirstOrDefault(s => serviceType.IsAssignableFrom(s));

            if (key == null)
            {
                throw new ServiceNotRegisteredException(typeof(TService));
            }

            return (TService)_services[key];
        }

        public bool TryGetService<TService>(out TService service) where TService : IService
        {
            var serviceType = typeof(TService);
            var key = _services.Keys.FirstOrDefault(s => serviceType.IsAssignableFrom(s));

            if (key == null)
            {
                service = default(TService);
                return false;
            }

            service = (TService)_services[key];
            return true;
        }

        public IList<IService> GetServices()
        {
            return _services.Values.ToList();
        }
    }
}
