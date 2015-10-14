using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Contracts;
using CK.HomeAutomation.Controller.Rooms;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.DHT22;
using CK.HomeAutomation.Hardware.GenericIOBoard;
using CK.HomeAutomation.Hardware.I2CHardwareBridge;
using CK.HomeAutomation.Hardware.OpenWeatherMapWeatherStation;
using CK.HomeAutomation.Hardware.Pi2;
using CK.HomeAutomation.Hardware.RemoteSwitch;
using CK.HomeAutomation.Hardware.RemoteSwitch.Codes;
using CK.HomeAutomation.Notifications;
using CK.HomeAutomation.Telemetry;

namespace CK.HomeAutomation.Controller
{
    internal class Controller : BaseController
    {
        protected override void Initialize()
        {
            InitializeHealthMonitor(22);

            var pi2PortController = new Pi2PortController();
            
            var i2CBus = new I2cBusAccessor(NotificationHandler);

            IWeatherStation weatherStation = CreateWeatherStation();

            var i2CHardwareBridge = new I2CHardwareBridge(50, i2CBus);
            var sensorBridgeDriver = new DHT22Accessor(i2CHardwareBridge, Timer);

            var ioBoardManager = new IOBoardManager(HttpApiController, NotificationHandler);
            var ccToolsBoardController = new CCToolsBoardController(i2CBus, ioBoardManager, NotificationHandler);

            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input0, 42);
            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input1, 43);
            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input2, 47);
            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input3, 45);
            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input4, 46);
            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input5, 44);

            RemoteSwitchController remoteSwitchController = SetupRemoteSwitchController(i2CHardwareBridge);

            var home = new Home(Timer, HealthMonitor, weatherStation, HttpApiController, NotificationHandler);

            new BedroomConfiguration().Setup(home, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new OfficeConfiguration().Setup(home, ccToolsBoardController, ioBoardManager, sensorBridgeDriver, remoteSwitchController);
            new UpperBathroomConfiguration().Setup(home, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new ReadingRoomConfiguration().Setup(home, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new ChildrensRoomRoomConfiguration().Setup(home, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new KitchenConfiguration().Setup(home, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new FloorConfiguration().Setup(home, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new LowerBathroomConfiguration().Setup(home, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new StoreroomConfiguration().Setup(home, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new LivingRoomConfiguration().Setup(home, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);

            home.PublishStatisticsNotification();

            AttachAzureEventHubPublisher(home);

            var localCsvFileWriter = new LocalCsvFileWriter(NotificationHandler);
            localCsvFileWriter.ConnectActuators(home);

            var ioBoardsInterruptMonitor = new InterruptMonitor(pi2PortController.GetInput(4));
            Timer.Tick += (s, e) => ioBoardsInterruptMonitor.Poll();
            ioBoardsInterruptMonitor.InterruptDetected += (s, e) => ioBoardManager.PollInputBoardStates();
        }

        private RemoteSwitchController SetupRemoteSwitchController(I2CHardwareBridge i2CHardwareBridge)
        {
            // TODO: PD7+3
            var ldp433MHzSender = new LPD433MHzSignalSender(i2CHardwareBridge, 10, HttpApiController);
            var remoteSwitchController = new RemoteSwitchController(ldp433MHzSender, Timer);

            var intertechnoCodes = new IntertechnoCodeSequenceProvider();
            var brennenstuhlCodes = new BrennenstuhlCodeSequenceProvider();

            remoteSwitchController.Register(
                0,
                intertechnoCodes.GetSequence(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1, RemoteSwitchCommand.TurnOn),
                intertechnoCodes.GetSequence(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1, RemoteSwitchCommand.TurnOff));

            //remoteSwitchController.Register(
            //    1,
            //    brennenstuhlCodes.GetSequence(BrennenstuhlCodeSequenceProvider.SystemCode.AllOn, BrennenstuhlCodeSequenceProvider.UnitCode.A,
            //        RemoteSwitchCommand.TurnOn),
            //    brennenstuhlCodes.GetSequence(BrennenstuhlCodeSequenceProvider.SystemCode.AllOn, BrennenstuhlCodeSequenceProvider.UnitCode.A,
            //        RemoteSwitchCommand.TurnOff));

            return remoteSwitchController;
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

        private IWeatherStation CreateWeatherStation()
        {
            try
            {
                var configuration = JsonObject.Parse(File.ReadAllText(Path.Combine(ApplicationData.Current.LocalFolder.Path, "WeatherStationConfiguration.json")));

                double lat = configuration.GetNamedNumber("lat");
                double lon = configuration.GetNamedNumber("lon");
                string appId = configuration.GetNamedString("appID");

                var weatherStation = new OWMWeatherStation(lat, lon, appId, Timer, HttpApiController, NotificationHandler);
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
