using Windows.ApplicationModel.Background;

namespace HA4IoT.Controller.Cellar
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            new Controller(22).RunAsync(taskInstance);
        }
    }
}