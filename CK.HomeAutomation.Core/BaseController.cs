using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using CK.HomeAutomation.Core.Timer;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Core
{
    public abstract class BaseController
    {
        private BackgroundTaskDeferral _deferral;
        private HttpServer _httpServer;

        protected INotificationHandler NotificationHandler { get; private set; }
        protected IHttpRequestController HttpApiController { get; private set; }
        protected IHomeAutomationTimer Timer { get; private set; }

        public void RunAsync(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            Task.Factory.StartNew(InitializeCore, TaskCreationOptions.LongRunning);
        }

        protected void InitializeHttpApi()
        {
            _httpServer = new HttpServer();
            var httpRequestDispatcher = new HttpRequestDispatcher(_httpServer);
            HttpApiController = httpRequestDispatcher.GetController("api");

            var appPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "app");
            httpRequestDispatcher.MapDirectory("app", appPath);
        }

        protected void InitializeTimer()
        {
            Timer = new HomeAutomationTimer(NotificationHandler);
        }

        protected void InitializeNotificationHandler()
        {
            NotificationHandler = new NotificationHandler();
            NotificationHandler.Publish(NotificationType.Info, "Starting");
        }

        protected virtual void Initialize()
        {
        }

        private void InitializeCore()
        {
            InitializeNotificationHandler();
            InitializeTimer();
            InitializeHttpApi();

            Initialize();
            
            _httpServer.StartAsync(80).Wait();
            Timer.Run();
        }
    }
}
