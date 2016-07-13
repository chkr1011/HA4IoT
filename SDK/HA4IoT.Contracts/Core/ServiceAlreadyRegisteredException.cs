using System;

namespace HA4IoT.Contracts.Core
{
    public class ServiceAlreadyRegisteredException : Exception
    {
        public ServiceAlreadyRegisteredException(Type serviceType)
            : base($"Service '{serviceType.Name}' is already registered.")
        {
        }
    }
}
