using System;

namespace HA4IoT.Contracts.Core
{
    public class ServiceNotRegisteredException : Exception
    {
        public ServiceNotRegisteredException(Type serviceType)
            : base($"Service '{serviceType.Name}' is not registered.")
        {
        }
    }
}
