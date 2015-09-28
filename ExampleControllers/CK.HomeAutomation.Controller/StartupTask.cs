using Windows.ApplicationModel.Background;

namespace CK.HomeAutomation.Controller
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            new Controller().RunAsync(taskInstance);
        }
    }
}