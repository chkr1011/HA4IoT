using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Controller.Main.Rooms;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
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
            
            AddDevice(new BuiltInI2CBus(Logger));
            AddDevice(new I2CHardwareBridge(new DeviceId("HB"), new I2CSlaveAddress(50), GetDevice<II2CBus>(), Timer));
            AddDevice(new OpenWeatherMapWeatherStation(OpenWeatherMapWeatherStation.DefaultDeviceId, Timer, HttpApiController, Logger));

            var ccToolsBoardController = new CCToolsBoardController(this, GetDevice<II2CBus>(), HttpApiController, Logger);
            
            var configurationParser = new ConfigurationParser(this);
            configurationParser.RegisterConfigurationExtender(new CCToolsConfigurationExtender(configurationParser, this));
            configurationParser.ParseConfiguration();
            
            ccToolsBoardController.CreateHSPE16InputOnly(Main.Device.Input0, new I2CSlaveAddress(42));
            ccToolsBoardController.CreateHSPE16InputOnly(Main.Device.Input1, new I2CSlaveAddress(43));
            ccToolsBoardController.CreateHSPE16InputOnly(Main.Device.Input2, new I2CSlaveAddress(47));
            ccToolsBoardController.CreateHSPE16InputOnly(Main.Device.Input3, new I2CSlaveAddress(45));
            ccToolsBoardController.CreateHSPE16InputOnly(Main.Device.Input4, new I2CSlaveAddress(46));
            ccToolsBoardController.CreateHSPE16InputOnly(Main.Device.Input5, new I2CSlaveAddress(44));

            RemoteSocketController remoteSwitchController = SetupRemoteSwitchController();
            
            new BedroomConfiguration(this, ccToolsBoardController).Setup();
            new OfficeConfiguration().Setup(this, ccToolsBoardController, remoteSwitchController);
            new UpperBathroomConfiguration(this, ccToolsBoardController).Setup();
            new ReadingRoomConfiguration().Setup(this, ccToolsBoardController);
            new ChildrensRoomRoomConfiguration().Setup(this, ccToolsBoardController);
            new KitchenConfiguration().Setup(this, ccToolsBoardController);
            new FloorConfiguration().Setup(this, ccToolsBoardController);
            new LowerBathroomConfiguration().Setup(this);
            new StoreroomConfiguration().Setup(this, ccToolsBoardController);
            new LivingRoomConfiguration().Setup(this, ccToolsBoardController);
            
            //AttachAzureEventHubPublisher(home);

            var localCsvFileWriter = new CsvHistory(Logger, HttpApiController);
            localCsvFileWriter.ConnectActuators(this);
            localCsvFileWriter.ExposeToApi(HttpApiController);

            var ioBoardsInterruptMonitor = new InterruptMonitor(pi2PortController.GetInput(4), Logger);

            ioBoardsInterruptMonitor.InterruptDetected += (s, e) => ccToolsBoardController.PollInputBoardStates();
            ioBoardsInterruptMonitor.StartPollingAsync();
        }

        private RemoteSocketController SetupRemoteSwitchController()
        {
            const int LDP433MhzSenderPin = 10;

            var i2cHardwareBridge = GetDevice<I2CHardwareBridge>();
            var brennenstuhl = new BrennenstuhlCodeSequenceProvider();
            var ldp433MHzSender = new LPD433MHzSignalSender(i2cHardwareBridge, LDP433MhzSenderPin, HttpApiController);

            var remoteSwitchController = new RemoteSocketController(new DeviceId("RemoteSocketController"),  ldp433MHzSender, Timer)
                .WithRemoteSocket(0, brennenstuhl.GetSequencePair(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A));

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
    }
}
