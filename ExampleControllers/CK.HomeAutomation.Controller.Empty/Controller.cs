using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Contracts;
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

namespace CK.HomeAutomation.Controller.Empty
{
    internal class Controller : BaseController
    {
        private const int LedGpio = 22;
        private const int I2CHardwareBridgeAddress = 50;
        private const byte I2CHardwareBridge433MHzSenderPin = 6;

        private enum Room
        {
            ExampleRoom
        }

        private enum ExampleRoom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            Lamp1,
            Lamp2,

            Lamp3,
            Lamp4,
            Lamp5,
            Lamp6,
            Lamp7,

            Socket1,
            Socket2,
            Socket3,
            Socket4,

            Window,

            VirtualButtonTest,
            LedStripRemote,

            Fan
        }

        private enum Device
        {
            HSPE16,
            HSRel8,
            HSRel5
        }

        protected override void Initialize()
        {
            // Setup the health monitor which tracks the average time and let an LED blink if everything is healthy.
            InitializeHealthMonitor(LedGpio);

            // Setup the controller which provides ports from the GPIOs of the Pi2.
            var pi2PortController = new Pi2PortController();

            // Setup the wrapper for I2C bus access.
            var i2CBus = new I2cBusAccessor(NotificationHandler);

            // Setup the manager for all types of IO boards which exposes all IO boards to the HTTP API
            // and polls the states of the inputs.
            var ioBoardManager = new IOBoardManager(HttpApiController, NotificationHandler);

            // Setup the controller which creates ports for IO boards from CCTools (or based on PCF8574/MAX7311/PCA9555D).
            var ccToolsBoardController = new CCToolsBoardController(i2CBus, ioBoardManager, NotificationHandler);
            ccToolsBoardController.CreateHSPE16InputOnly(Device.HSPE16, 41);
            ccToolsBoardController.CreateHSREL8(Device.HSRel8, 40);
            ccToolsBoardController.CreateHSREL5(Device.HSRel5, 56);

            // Setup the remote switch 433Mhz sender which is attached to the I2C bus (Arduino Nano).
            var i2CHardwareBridge = new I2CHardwareBridge(I2CHardwareBridgeAddress, i2CBus);
            var remoteSwitchSender = new LPD433MHzSignalSender(i2CHardwareBridge, I2CHardwareBridge433MHzSenderPin, HttpApiController);
            var dht22Accessor = new DHT22Accessor(i2CHardwareBridge, Timer);

            // Setup the controller which creates ports for wireless sockets (433Mhz).
            var remoteSwitchController = new RemoteSwitchController(remoteSwitchSender, Timer);
            var intertechnoCodes = new IntertechnoCodeSequenceProvider();
            var brennenstuhlCodes = new BrennenstuhlCodeSequenceProvider();

            remoteSwitchController.Register(
                0,
                brennenstuhlCodes.GetSequence(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A, RemoteSwitchCommand.TurnOn),
                brennenstuhlCodes.GetSequence(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A, RemoteSwitchCommand.TurnOff));

            ////remoteSwitchController.Register(
            ////    0,
            ////    intertechnoCodes.GetSequence(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1, RemoteSwitchCommand.TurnOn),
            ////    intertechnoCodes.GetSequence(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1, RemoteSwitchCommand.TurnOff));

            // Setup the weather station which provides sunrise and sunset information.
            var weatherStation = CreateWeatherStation();
            
            var home = new Home(Timer, HealthMonitor, weatherStation, HttpApiController, NotificationHandler);

            // Add new rooms with actuators here! Example:
            var exampleRoom = home.AddRoom(Room.ExampleRoom)
                .WithTemperatureSensor(ExampleRoom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(5))
                .WithHumiditySensor(ExampleRoom.HumiditySensor, dht22Accessor.GetHumiditySensor(5))
                .WithMotionDetector(ExampleRoom.MotionDetector, ioBoardManager.GetInputBoard(Device.HSPE16).GetInput(8))
                .WithWindow(ExampleRoom.Window, w => w.WithCenterCasement(ioBoardManager.GetInputBoard(Device.HSPE16).GetInput(0)))
                .WithLamp(ExampleRoom.Lamp1, remoteSwitchController.GetOutput(0))
                .WithSocket(ExampleRoom.Socket1, ioBoardManager.GetOutputBoard(Device.HSRel5).GetOutput(0))
                .WithSocket(ExampleRoom.Socket2, ioBoardManager.GetOutputBoard(Device.HSRel5).GetOutput(1))
                .WithSocket(ExampleRoom.Socket3, ioBoardManager.GetOutputBoard(Device.HSRel5).GetOutput(2))
                .WithSocket(ExampleRoom.Fan, ioBoardManager.GetOutputBoard(Device.HSRel5).GetOutput(3))
                .WithLamp(ExampleRoom.Lamp2, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(0))
                .WithLamp(ExampleRoom.Lamp3, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(1))
                .WithLamp(ExampleRoom.Lamp4, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(2))
                .WithLamp(ExampleRoom.Lamp5, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(3))
                .WithLamp(ExampleRoom.Lamp6, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(4))
                .WithLamp(ExampleRoom.Lamp7, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(5))
                .WithVirtualButtonGroup(ExampleRoom.LedStripRemote, g => SetupLEDStripRemote(i2CHardwareBridge, g));
            
            exampleRoom.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(exampleRoom.MotionDetector(ExampleRoom.MotionDetector))
                .WithTarget(exampleRoom.Lamp(ExampleRoom.Lamp2))
                .WithOnDuration(TimeSpan.FromSeconds(10));

            exampleRoom.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(exampleRoom.MotionDetector(ExampleRoom.MotionDetector))
                .WithTarget(exampleRoom.BinaryStateOutput(ExampleRoom.Fan))
                .WithOnDuration(TimeSpan.FromSeconds(10));

            home.PublishStatisticsNotification();

            // Setup the CSV writer which writes all state changes to the SD card (package directory).
            var localCsvFileWriter = new LocalCsvFileWriter(NotificationHandler);
            localCsvFileWriter.ConnectActuators(home);

            Timer.Tick += (s, e) =>
            {
                pi2PortController.PollOpenInputPorts();
                ioBoardManager.PollInputBoardStates();
            };
        }

        private void SetupLEDStripRemote(I2CHardwareBridge i2CHardwareBridge, VirtualButtonGroup group)
        {
            var ledStripRemote = new LEDStripRemote(i2CHardwareBridge, 4);

            group.WithButton("on", b => b.WithShortAction(() => ledStripRemote.TurnOn()))
                .WithButton("off", b => b.WithShortAction(() => ledStripRemote.TurnOff()))
                .WithButton("white", b => b.WithShortAction(() => ledStripRemote.TurnWhite()))

                .WithButton("red1", b => b.WithShortAction(() => ledStripRemote.TurnRed1()))
                .WithButton("green1", b => b.WithShortAction(() => ledStripRemote.TurnGreen1()))
                .WithButton("blue1", b => b.WithShortAction(() => ledStripRemote.TurnBlue1()));
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
                NotificationHandler.PublishFrom(this, NotificationType.Info, "WeatherStation initialized successfully");

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
