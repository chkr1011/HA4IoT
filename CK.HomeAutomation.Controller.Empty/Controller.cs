using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.GenericIOBoard;
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
        private const int RemoteSwitchSenderAddress = 50;

        private enum Room
        {
            Room1
        }

        private enum Room1Actuator
        {
            Lamp1,
            Socket1
        }

        private enum RelayBoard
        {
            Board1
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
            ccToolsBoardController.CreateHSREL5(RelayBoard.Board1, 37);

            // Setup the remote switch 433Mhz sender which is attached to the I2C bus (Arduino Nano).
            var remoteSwitchSender = new LPD433MhzSignalSender(i2CBus, RemoteSwitchSenderAddress, HttpApiController);

            // Setup the controller which creates ports for wireless sockets (433Mhz).
            var remoteSwitchController = new RemoteSwitchController(remoteSwitchSender, Timer);
            var intertechnoCodes = new IntertechnoCodeSequenceProvider();
            remoteSwitchController.Register(
                0, 
                intertechnoCodes.GetSequence(IntertechnoCodeSequenceProvider.SystemCode.A, 1, RemoteSwitchCommand.TurnOn),
                intertechnoCodes.GetSequence(IntertechnoCodeSequenceProvider.SystemCode.A, 1, RemoteSwitchCommand.TurnOff));
            
            // Setup the weather station which provides sunrise and sunset information.
            double lat = 52.5075419;
            double lon = 13.4251364;
            var weatherStation = new WeatherStation(lat, lon, Timer, HttpApiController, NotificationHandler);
            NotificationHandler.PublishFrom(this, NotificationType.Info, "WeatherStation initialized successfully.");

            var home = new Home(Timer, HealthMonitor, weatherStation, HttpApiController, NotificationHandler);

            // Add new rooms with actuators here!
            // Example:
            var room1 = home.AddRoom(Room.Room1)
                .WithLamp(Room1Actuator.Lamp1, remoteSwitchController.GetOutput(0))
                .WithSocket(Room1Actuator.Socket1, ioBoardManager.GetOutputBoard(RelayBoard.Board1).GetOutput(0));

            // Setup the CSV writer which writes all state changes to the SD card (package directory).
            var localCsvFileWriter = new LocalCsvFileWriter(NotificationHandler);
            localCsvFileWriter.ConnectActuators(home);

            home.PublishStatisticsNotification();

            Timer.Tick += (s, e) =>
            {
                pi2PortController.PollOpenInputPorts();
                ioBoardManager.PollInputBoardStates();
            };
        }
    }
}
