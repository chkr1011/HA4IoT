using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core;

namespace HA4IoT.Controller
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var options = new ControllerOptions
            {
                StatusLedGpio = 22, // Replace this with a port which contains a LED for status indication (optional).
                ConfigurationType = typeof(Configuration)
            };

            var controller = new Core.Controller(options);
            controller.RunAsync(taskInstance);
        }

        private class Configuration : IConfiguration
        {
            private readonly ILogger _log;

            public Configuration(ILogService logService)
            {
                // Use the constructor for injection of services.
                // To resolve services search for implementations of IService.
                _log = logService.CreatePublisher(nameof(Configuration));
                _log.Info("Hello World.");
            }

            public Task ApplyAsync()
            {
                // Appy area and component configuration here.
                _log.Info("Foo bar.");

                return Task.FromResult(0);
            }
        }
    }
}
