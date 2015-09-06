using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Devices.Gpio;
using Windows.Storage;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Controller.Rooms;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.Drivers;
using CK.HomeAutomation.Hardware.Pi2;
using CK.HomeAutomation.Hardware.RemoteSwitch;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;
using CK.HomeAutomation.Telemetry;

namespace CK.HomeAutomation.Controller
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void Run()
        {
            var notificationHandler = new NotificationHandler();
            notificationHandler.Publish(NotificationType.Info, "Starting");

            var timer = new HomeAutomationTimer(notificationHandler);

            var httpServer = new HttpServer();
            var httpRequestDispatcher = new HttpRequestDispatcher(httpServer);
            var httpApiController = httpRequestDispatcher.GetController("api");

            var i2CBus = new I2CBus(notificationHandler);
            var pi2PortManager = new Pi2PortManager();
            var healthMonitor = new HealthMonitor(pi2PortManager.GetOutput(22).WithInvertedState(), timer, httpApiController);

            WeatherStation weatherStation = CreateWeatherStation(timer, httpApiController, notificationHandler);
            
            var sensorBridgeDriver = new TemperatureAndHumiditySensorBridgeDriver(50, timer, i2CBus);

            var ioBoardManager = new IOBoardManager(httpApiController, notificationHandler);
            var ccToolsFactory = new CCToolsFactory(i2CBus, ioBoardManager, notificationHandler);
            ccToolsFactory.CreateHSPE16InputOnly(Device.Input0, 42);
            ccToolsFactory.CreateHSPE16InputOnly(Device.Input1, 43);
            ccToolsFactory.CreateHSPE16InputOnly(Device.Input2, 47);
            ccToolsFactory.CreateHSPE16InputOnly(Device.Input3, 45);
            ccToolsFactory.CreateHSPE16InputOnly(Device.Input4, 46);
            ccToolsFactory.CreateHSPE16InputOnly(Device.Input5, 44);

            var remoteSwitchController = new RemoteSwitchController(new RemoteSwitchBridgeDriver(i2CBus, 50), timer);
            remoteSwitchController.Register(0, new RemoteSwitchCode(21, 24), new RemoteSwitchCode(20, 24));

            var home = new Home(timer, healthMonitor, weatherStation, httpApiController, notificationHandler);

            new BedroomConfiguration().Setup(home, ioBoardManager, ccToolsFactory, sensorBridgeDriver);
            new OfficeConfiguration().Setup(home, ioBoardManager, ccToolsFactory, sensorBridgeDriver, remoteSwitchController);
            new UpperBathroomConfiguration().Setup(home, ioBoardManager, ccToolsFactory, sensorBridgeDriver);
            new ReadingRoomConfiguration().Setup(home, ioBoardManager, ccToolsFactory, sensorBridgeDriver);
            new ChildrensRoomRoomConfiguration().Setup(home, ioBoardManager, ccToolsFactory, sensorBridgeDriver);
            new KitchenConfiguration().Setup(home, ioBoardManager, ccToolsFactory, sensorBridgeDriver);
            new FloorConfiguration().Setup(home, ioBoardManager, ccToolsFactory, sensorBridgeDriver);
            new LowerBathroomConfiguration().Setup(home, ioBoardManager, sensorBridgeDriver);
            new StoreroomConfiguration().Setup(home, ioBoardManager, ccToolsFactory, sensorBridgeDriver);
            new LivingRoomConfiguration().Setup(home, ioBoardManager, ccToolsFactory, sensorBridgeDriver);
            home.PublishStatisticsNotification();

            AttachAzureEventHubPublisher(home, notificationHandler);
            
            var localCsvFileWriter = new LocalCsvFileWriter(notificationHandler);
            localCsvFileWriter.ConnectActuators(home);

            var ioBoardsInterruptMonitor = new InterruptMonitor(GpioController.GetDefault().OpenPin(4), timer);
            ioBoardsInterruptMonitor.InterruptDetected += (s, e) => ioBoardManager.PollInputBoardStates();

            httpServer.StartAsync(80).Wait();
            timer.Run();
        }

        private void AttachAzureEventHubPublisher(Home home, NotificationHandler notificationHandler)
        {
            try
            {
                var configuration = JsonObject.Parse(File.ReadAllText(Path.Combine(ApplicationData.Current.LocalFolder.Path, "EventHubConfiguration.json")));

                var azureEventHubPublisher = new AzureEventHubPublisher(
                    configuration.GetNamedString("eventHubNamespace"),
                    configuration.GetNamedString("eventHubName"),
                    configuration.GetNamedString("sasToken"),
                    notificationHandler);

                azureEventHubPublisher.ConnectActuators(home);
                notificationHandler.PublishFrom(this, NotificationType.Info, "AzureEventHubPublisher initialized successfully.");
            }
            catch (Exception exception)
            {
                notificationHandler.PublishFrom(this, NotificationType.Warning, "Unable to create azure event hub publisher. " + exception.Message);
            }
        }

        private WeatherStation CreateWeatherStation(HomeAutomationTimer timer, HttpRequestController httpApiController, INotificationHandler notificationHandler)
        {
            try
            {
                var configuration = JsonObject.Parse(File.ReadAllText(Path.Combine(ApplicationData.Current.LocalFolder.Path, "WeatherStationConfiguration.json")));

                double lat = configuration.GetNamedNumber("lat");
                double lon = configuration.GetNamedNumber("lon");
                
                var weatherStation = new WeatherStation(lat, lon, timer, httpApiController, notificationHandler);
                notificationHandler.PublishFrom(this, NotificationType.Info, "WeatherStation initialized successfully.");
                return weatherStation;
            }
            catch (Exception exception)
            {
                notificationHandler.PublishFrom(this, NotificationType.Warning, "Unable to create weather station. " + exception.Message);
            }

            return null;
        }
    }
}