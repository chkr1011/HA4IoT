using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public class ServiceRegistration
    {
        public ServiceRegistration(Type interfaceType, IService serviceInstance)
        {
            if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));
            if (serviceInstance == null) throw new ArgumentNullException(nameof(serviceInstance));

            InterfaceType = interfaceType;
            ServiceInstance = serviceInstance;
        }

        public Type InterfaceType { get; }

        public IService ServiceInstance { get; }
    }
}
