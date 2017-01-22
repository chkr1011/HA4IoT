using Windows.ApplicationModel.Background;
using HA4IoT.Core;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Core;
using System;
using System.Threading.Tasks;

namespace HA4IoT.Controller.Default
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const int LedGpio = 22;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var options = new ControllerOptions
            {
                StatusLedNumber = LedGpio
            };

            var controller = new Core.Controller(options);
            controller.RunAsync(taskInstance);
        }

        private class Configuration : IConfiguration
        {
            private readonly IPi2GpioService _pi2GpioService;

            public Configuration(IPi2GpioService pi2GpioService)
            {
                if (pi2GpioService == null) throw new ArgumentNullException(nameof(pi2GpioService));

                _pi2GpioService = pi2GpioService;
            }

            public Task ApplyAsync()
            {
                return Task.FromResult(0);
            }
        }
    }
}
