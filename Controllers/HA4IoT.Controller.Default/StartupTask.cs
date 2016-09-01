using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Core;

namespace HA4IoT.Controller.Default
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const int LedGpio = 22;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var controller = new Core.Controller(new ControllerOptions { StatusLedNumber = LedGpio, Configuration = new Configuration() });
            controller.RunAsync(taskInstance);
        }

        private class Configuration : IConfiguration
        {
            public void SetupContainer(IContainerService containerService)
            {
            }

            public Task Configure(IContainerService containerService)
            {
                return Task.FromResult(0);
            }
        }
    }
}
