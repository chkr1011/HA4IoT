using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Controller.Rooms;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.DHT22;
using CK.HomeAutomation.Hardware.Pi2;
using CK.HomeAutomation.Hardware.RemoteSwitch;
using CK.HomeAutomation.Notifications;
using CK.HomeAutomation.Telemetry;

namespace CK.HomeAutomation.Controller
{
    internal class Controller : BaseController
    {
        protected override void Initialize()
        {
            var pi2PortController = new Pi2PortController();
            var healthMonitor = new HealthMonitor(pi2PortController.GetOutput(22).WithInvertedState(), Timer, HttpApiController);

            var i2CBus = new I2CBus(NotificationHandler);

            WeatherStation weatherStation = CreateWeatherStation();

            var sensorBridgeDriver = new DHT22Reader(50, Timer, i2CBus);

            var ccToolsController = new CCToolsBoardController(i2CBus, HttpApiController, NotificationHandler);
            ccToolsController.CreateHSPE16InputOnly(Device.Input0, 42);
            ccToolsController.CreateHSPE16InputOnly(Device.Input1, 43);
            ccToolsController.CreateHSPE16InputOnly(Device.Input2, 47);
            ccToolsController.CreateHSPE16InputOnly(Device.Input3, 45);
            ccToolsController.CreateHSPE16InputOnly(Device.Input4, 46);
            ccToolsController.CreateHSPE16InputOnly(Device.Input5, 44);

            var remoteSwitchController = new RemoteSwitchController(new RemoteSwitchSender(i2CBus, 50), Timer);
            remoteSwitchController.Register(0, new RemoteSwitchCode(21, 24), new RemoteSwitchCode(20, 24));

            var home = new Home(Timer, healthMonitor, weatherStation, HttpApiController, NotificationHandler);

            new BedroomConfiguration().Setup(home, ccToolsController, sensorBridgeDriver);
            new OfficeConfiguration().Setup(home, ccToolsController, sensorBridgeDriver, remoteSwitchController);
            new UpperBathroomConfiguration().Setup(home, ccToolsController, sensorBridgeDriver);
            new ReadingRoomConfiguration().Setup(home, ccToolsController, sensorBridgeDriver);
            new ChildrensRoomRoomConfiguration().Setup(home, ccToolsController, sensorBridgeDriver);
            new KitchenConfiguration().Setup(home, ccToolsController, sensorBridgeDriver);
            new FloorConfiguration().Setup(home, ccToolsController, sensorBridgeDriver);
            new LowerBathroomConfiguration().Setup(home, ccToolsController, sensorBridgeDriver);
            new StoreroomConfiguration().Setup(home, ccToolsController, sensorBridgeDriver);
            new LivingRoomConfiguration().Setup(home, ccToolsController, sensorBridgeDriver);
            home.PublishStatisticsNotification();

            AttachAzureEventHubPublisher(home);

            var localCsvFileWriter = new LocalCsvFileWriter(NotificationHandler);
            localCsvFileWriter.ConnectActuators(home);

            var ioBoardsInterruptMonitor = new InterruptMonitor(pi2PortController.GetInput(4));
            Timer.Tick += (s, e) => ioBoardsInterruptMonitor.Poll();
            ioBoardsInterruptMonitor.InterruptDetected += (s, e) => ccToolsController.PollInputBoardStates();
        }

        private void AttachAzureEventHubPublisher(Home home)
        {
            try
            {
                var configuration = JsonObject.Parse(File.ReadAllText(Path.Combine(ApplicationData.Current.LocalFolder.Path, "EventHubConfiguration.json")));

                var azureEventHubPublisher = new AzureEventHubPublisher(
                    configuration.GetNamedString("eventHubNamespace"),
                    configuration.GetNamedString("eventHubName"),
                    configuration.GetNamedString("sasToken"),
                    NotificationHandler);

                azureEventHubPublisher.ConnectActuators(home);
                NotificationHandler.PublishFrom(this, NotificationType.Info, "AzureEventHubPublisher initialized successfully.");
            }
            catch (Exception exception)
            {
                NotificationHandler.PublishFrom(this, NotificationType.Warning, "Unable to create azure event hub publisher. " + exception.Message);
            }
        }

        private WeatherStation CreateWeatherStation()
        {
            try
            {
                var configuration = JsonObject.Parse(File.ReadAllText(Path.Combine(ApplicationData.Current.LocalFolder.Path, "WeatherStationConfiguration.json")));

                double lat = configuration.GetNamedNumber("lat");
                double lon = configuration.GetNamedNumber("lon");

                var weatherStation = new WeatherStation(lat, lon, Timer, HttpApiController, NotificationHandler);
                NotificationHandler.PublishFrom(this, NotificationType.Info, "WeatherStation initialized successfully.");
                return weatherStation;
            }
            catch (Exception exception)
            {
                NotificationHandler.PublishFrom(this, NotificationType.Warning, "Unable to create weather station. " + exception.Message);
            }

            return null;
        }
    }
}
