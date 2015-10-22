using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
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
        protected HealthMonitor HealthMonitor { get; private set; }

        public void RunAsync(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            Task.Factory.StartNew(InitializeCore, TaskCreationOptions.LongRunning);
        }

        protected virtual void Initialize()
        {
        }

        protected void InitializeHealthMonitor(int pi2GpioPinWithLed)
        {
            var pi2PortController = GpioController.GetDefault();
            var ledPin = pi2PortController.OpenPin(pi2GpioPinWithLed);
            ledPin.SetDriveMode(GpioPinDriveMode.Output);

            HealthMonitor = new HealthMonitor(ledPin, Timer, HttpApiController);
        }

        private void InitializeHttpApi()
        {
            _httpServer = new HttpServer();
            var httpRequestDispatcher = new HttpRequestDispatcher(_httpServer);
            HttpApiController = httpRequestDispatcher.GetController("api");

            var appPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "app");
            httpRequestDispatcher.MapFolder("app", appPath);
        }

        private void InitializeTimer()
        {
            Timer = new HomeAutomationTimer(NotificationHandler);
        }

        private void InitializeNotificationHandler()
        {
            NotificationHandler = new NotificationHandler();
            NotificationHandler.Publish(NotificationType.Info, "Starting");
        }

        private void InitializeCore()
        {
            InitializeNotificationHandler();
            InitializeTimer();
            InitializeHttpApi();
            TryInitializeActuators();

            _httpServer.StartAsync(80).Wait();
            Timer.Run();
        }

        private void TryInitializeActuators()
        {
            try
            {
                Initialize();
            }
            catch (Exception exception)
            {
                NotificationHandler.PublishFrom(this, NotificationType.Error, "Error while initializing. " + exception);
            }
        }
    }
}
