using System;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Core
{
    public static class ContainerExtensions
    {
        public static void ExposeRegistrationsToApi(this Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            var apiService = container.GetInstance<IApiDispatcherService>();
            foreach (var registration in container.GetCurrentRegistrations())
            {
                apiService.Expose(registration.GetInstance());
            }
        }

        public static void StartupServices(this IContainer container, ILogger log)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (log == null) throw new ArgumentNullException(nameof(log));

            foreach (var service in container.GetInstances<IService>())
            {
                try
                {
                    service.Startup();
                }
                catch (Exception exception)
                {
                    log.Error(exception, $"Error while starting service '{service.GetType().Name}'. " + exception.Message);
                }
            }
        }
    }
}
