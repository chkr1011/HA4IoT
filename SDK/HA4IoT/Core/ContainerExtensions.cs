using System.Collections.Generic;
using System.Reflection;
using SimpleInjector;

namespace HA4IoT.Core
{
    public static class ContainerExtensions
    {
        public static IList<InstanceProducer> GetRegistrationsOf<TService>(this Container container) where TService : class 
        {
            var services = new List<InstanceProducer>();

            foreach (var registration in container.GetCurrentRegistrations())
            {
                if (typeof(TService).IsAssignableFrom(registration.ServiceType))
                {
                    services.Add(registration);
                }
            }
            
            return services;
        }
    }
}
