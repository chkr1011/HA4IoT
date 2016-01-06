using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Controller.Main.Rooms;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.DHT22;
using HA4IoT.Hardware.GenericIOBoard;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.OpenWeatherMapWeatherStation;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Codes;
using HA4IoT.Telemetry.Azure;
using HA4IoT.Telemetry.Csv;

namespace HA4IoT.Controller.Main
{
    internal class Controller : ControllerBase
    {
        protected override void Initialize()
        {
            InitializeHealthMonitor(22);

            var pi2PortController = new Pi2PortController();
            
            var i2CBus = new I2CBusWrapper(Logger);

            InitializeWeatherStation(CreateWeatherStation());

            var i2CHardwareBridge = new I2CHardwareBridge(new I2CSlaveAddress(50), i2CBus);
            var sensorBridgeDriver = new DHT22Accessor(i2CHardwareBridge, Timer);

            var ioBoardManager = new IOBoardCollection(HttpApiController, Logger);
            var ccToolsBoardController = new CCToolsBoardController(i2CBus, ioBoardManager, Logger);

            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input0, new I2CSlaveAddress(42));
            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input1, new I2CSlaveAddress(43));
            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input2, new I2CSlaveAddress(47));
            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input3, new I2CSlaveAddress(45));
            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input4, new I2CSlaveAddress(46));
            ccToolsBoardController.CreateHSPE16InputOnly(Device.Input5, new I2CSlaveAddress(44));

            RemoteSwitchController remoteSwitchController = SetupRemoteSwitchController(i2CHardwareBridge);
            
            new BedroomConfiguration(ccToolsBoardController, ioBoardManager).Setup(this, sensorBridgeDriver);
            new OfficeConfiguration().Setup(this, ccToolsBoardController, ioBoardManager, sensorBridgeDriver, remoteSwitchController);
            new UpperBathroomConfiguration(ioBoardManager, ccToolsBoardController).Setup(this, sensorBridgeDriver);
            new ReadingRoomConfiguration().Setup(this, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new ChildrensRoomRoomConfiguration().Setup(this, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new KitchenConfiguration().Setup(this, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new FloorConfiguration().Setup(this, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new LowerBathroomConfiguration().Setup(this, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);
            new StoreroomConfiguration().Setup(this, ccToolsBoardController, ioBoardManager);
            new LivingRoomConfiguration().Setup(this, ccToolsBoardController, ioBoardManager, sensorBridgeDriver);

            PublishStatisticsNotification();

            //AttachAzureEventHubPublisher(home);

            var localCsvFileWriter = new CsvHistory(Logger, HttpApiController);
            localCsvFileWriter.ConnectActuators(this);
            localCsvFileWriter.ExposeToApi(HttpApiController);

            var ioBoardsInterruptMonitor = new InterruptMonitor(pi2PortController.GetInput(4), Logger);
            ioBoardsInterruptMonitor.StartPollingTaskAsync();

            ioBoardsInterruptMonitor.InterruptDetected += (s, e) => ioBoardManager.PollInputBoardStates();
        }

        private RemoteSwitchController SetupRemoteSwitchController(I2CHardwareBridge i2CHardwareBridge)
        {
            const int LDP433MhzSenderPin = 10;

            var ldp433MHzSender = new LPD433MHzSignalSender(i2CHardwareBridge, LDP433MhzSenderPin, HttpApiController);
            var remoteSwitchController = new RemoteSwitchController(ldp433MHzSender, Timer);
            
            var brennenstuhlCodes = new BrennenstuhlCodeSequenceProvider();
            remoteSwitchController.Register(
                0,
                brennenstuhlCodes.GetSequence(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A, RemoteSwitchCommand.TurnOn),
                brennenstuhlCodes.GetSequence(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A, RemoteSwitchCommand.TurnOff));

            return remoteSwitchController;
        }

        private void AttachAzureEventHubPublisher(IController controller)
        {
            try
            {
                var configuration = JsonObject.Parse(File.ReadAllText(Path.Combine(ApplicationData.Current.LocalFolder.Path, "EventHubConfiguration.json")));

                var azureEventHubPublisher = new AzureEventHubPublisher(
                    configuration.GetNamedString("eventHubNamespace"),
                    configuration.GetNamedString("eventHubName"),
                    configuration.GetNamedString("sasToken"),
                    Logger);

                azureEventHubPublisher.ConnectActuators(controller);
                Logger.Info("AzureEventHubPublisher initialized successfully.");
            }
            catch (Exception exception)
            {
                Logger.Warning("Unable to create azure event hub publisher. " + exception.Message);
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

                var weatherStation = new OWMWeatherStation(lat, lon, appId, Timer, HttpApiController, Logger);
                Logger.Info("WeatherStation initialized successfully.");
                return weatherStation;
            }
            catch (Exception exception)
            {
                Logger.Warning("Unable to create weather station. " + exception.Message);
            }

            return null;
        }
    }
}
