using Windows.ApplicationModel.Background;

namespace HA4IoT.Controller.Empty
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            new Controller().RunAsync(taskInstance);
        }
    }
}
