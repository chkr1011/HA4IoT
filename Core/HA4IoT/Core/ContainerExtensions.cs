using System;
using System.Collections.Generic;
using System.Reflection;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
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

        public static void ExposeRegistrationsToApi(this Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            var apiService = container.GetInstance<IApiDispatcherService>();
            foreach (var registration in container.GetCurrentRegistrations())
            {
                apiService.Expose(registration.GetInstance());
            }
        }

        public static void StartupServices(this Container container, ILogger log)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (log == null) throw new ArgumentNullException(nameof(log));

            foreach (var registration in container.GetRegistrationsOf<IService>())
            {
                try
                {
                    ((IService)registration.GetInstance()).Startup();
                }
                catch (Exception exception)
                {
                    log.Error(exception, $"Error while starting service '{registration.ServiceType.Name}'. " + exception.Message);
                }
            }
        }
    }
}
