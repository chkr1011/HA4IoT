using System;
using Windows.ApplicationModel.Background;
using HA4IoT.Core;

namespace HA4IoT.Controller.Main
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var hostname = System.Net.Dns.GetHostName();

            Type configurationType;
            if (hostname == "HA4IoT-Main")
            {
                configurationType = typeof(Main.Configuration);
            }
            else if (hostname == "HA4IoT-Cellar")
            {
                configurationType = typeof(Cellar.Configuration);
            }
            else
            {
                return;
            }

            var options = new ControllerOptions
            {
                StatusLedGpio = 22,
                ConfigurationType = configurationType
            };

            var controller = new Core.Controller(options);
            controller.RunAsync(taskInstance);
        }
    }
}