using System.Collections.Generic;
using System.Reflection;
using SimpleInjector;

namespace HA4IoT.Core
{
    public static class ContainerExtensions
    {
        public static IList<TService> GetImplementationsOf<TService>(this Container container)
        {
            var services = new List<TService>();

            foreach (var registration in container.GetCurrentRegistrations())
            {
                if (typeof(TService).IsAssignableFrom(registration.ServiceType))
                {
                    services.Add((TService)registration.GetInstance());
                }
            }

            return services;
        }
    }
}
