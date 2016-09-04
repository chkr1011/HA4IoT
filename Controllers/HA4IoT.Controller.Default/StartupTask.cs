using Windows.ApplicationModel.Background;
using HA4IoT.Core;

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
    }
}
