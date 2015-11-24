using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
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
using HA4IoT.Telemetry.Csv;

namespace HA4IoT.Controller.Demo
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

            Socket1,
            Socket2,

            Window,

            LedStripRemote,

            BathroomFan,
            CeilingFan,

            Button1,
            Button2
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
            var ioBoardManager = new IOBoardCollection(HttpApiController, NotificationHandler);

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
            remoteSwitchController.Register(
                0,
                intertechnoCodes.GetSequence(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1, RemoteSwitchCommand.TurnOn),
                intertechnoCodes.GetSequence(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1, RemoteSwitchCommand.TurnOff));

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
                .WithSocket(ExampleRoom.Socket2, ioBoardManager.GetOutputBoard(Device.HSRel5).GetOutput(4))
                .WithSocket(ExampleRoom.BathroomFan, ioBoardManager.GetOutputBoard(Device.HSRel5).GetOutput(3))
                .WithLamp(ExampleRoom.Lamp2, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(0))
                .WithLamp(ExampleRoom.Lamp3, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(1))
                .WithLamp(ExampleRoom.Lamp4, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(2))
                .WithLamp(ExampleRoom.Lamp5, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(3))
                .WithLamp(ExampleRoom.Lamp6, ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(4))
                .WithButton(ExampleRoom.Button1, ioBoardManager.GetInputBoard(Device.HSPE16).GetInput(1))
                .WithButton(ExampleRoom.Button2, ioBoardManager.GetInputBoard(Device.HSPE16).GetInput(2))
                .WithVirtualButtonGroup(ExampleRoom.LedStripRemote, g => SetupLEDStripRemote(i2CHardwareBridge, g))
                .WithStateMachine(ExampleRoom.CeilingFan, (sm, r) => SetupCeilingFan(sm, r, ioBoardManager));
            
            exampleRoom.Lamp(ExampleRoom.Lamp5).ConnectToggleActionWith(exampleRoom.Button(ExampleRoom.Button1));
            exampleRoom.Lamp(ExampleRoom.Lamp6).ConnectToggleActionWith(exampleRoom.Button(ExampleRoom.Button1), ButtonPressedDuration.Long);
            exampleRoom.StateMachine(ExampleRoom.CeilingFan).ConnectMoveNextAndToggleOffWith(exampleRoom.Button(ExampleRoom.Button2));

            SetupHumidityDependingOutput(exampleRoom.HumiditySensor(ExampleRoom.HumiditySensor), ioBoardManager.GetOutputBoard(Device.HSRel8).GetOutput(5));

            exampleRoom.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(exampleRoom.MotionDetector(ExampleRoom.MotionDetector))
                .WithTarget(exampleRoom.BinaryStateOutput(ExampleRoom.BathroomFan))
                .WithTarget(exampleRoom.BinaryStateOutput(ExampleRoom.Lamp2))
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

        private void SetupHumidityDependingOutput(IHumiditySensor sensor, IBinaryOutput output)
        {
            sensor.ValueChanged += (s, e) =>
            {
                if (e.NewValue > 80.0F)
                {
                    output.Write(BinaryState.High);
                }
                else
                {
                    output.Write(BinaryState.Low);
                }
            };
        }

        private void SetupCeilingFan(StateMachine stateMachine, Actuators.Room room, IOBoardCollection ioBoardManager)
        {
            var relayBoard = ioBoardManager.GetOutputBoard(Device.HSRel5);
            var gear1 = relayBoard.GetOutput(2);
            var gear2 = relayBoard.GetOutput(1);

            stateMachine.AddOffState().WithLowPort(gear1).WithLowPort(gear2);

            stateMachine.AddState("1").WithHighPort(gear1).WithLowPort(gear2);
            stateMachine.AddState("2").WithLowPort(gear1).WithHighPort(gear2);
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
                NotificationHandler.Info("WeatherStation initialized successfully");

                return weatherStation;
            }
            catch (Exception exception)
            {
                NotificationHandler.Warning("Unable to create weather station. " + exception.Message);
            }

            return null;
        }
    }
}
