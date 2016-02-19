using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.OpenWeatherMapWeatherStation;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Codes;
using HA4IoT.Telemetry.Csv;

namespace HA4IoT.Controller.Demo
{
    internal class Controller : ControllerBase
    {
        private const int LedGpio = 22;
        private static readonly I2CSlaveAddress I2CHardwareBridgeAddress = new I2CSlaveAddress(50);
        private const byte I2CHardwareBridge433MHzSenderPin = 6;

        private enum RoomId
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
            WeatherStation,
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
            var i2CBus = new DefaultI2CBus("II2CBus.default".ToDeviceId(), Logger);

            // Setup the controller which creates ports for IO boards from CCTools (or based on PCF8574/MAX7311/PCA9555D).
            var ccToolsBoardController = new CCToolsBoardController(this, i2CBus, HttpApiController, Logger);
            var hspe16 = ccToolsBoardController.CreateHSPE16InputOnly(Device.HSPE16, new I2CSlaveAddress(41));
            var hsrel8 = ccToolsBoardController.CreateHSREL8(Device.HSRel8, new I2CSlaveAddress(40));
            var hsrel5 = ccToolsBoardController.CreateHSREL5(Device.HSRel5, new I2CSlaveAddress(56));

            // Setup the remote switch 433Mhz sender which is attached to the I2C bus (Arduino Nano).
            var i2CHardwareBridge = new I2CHardwareBridge(new DeviceId("HB"),  I2CHardwareBridgeAddress, i2CBus, Timer);
            var remoteSwitchSender = new LPD433MHzSignalSender(i2CHardwareBridge, I2CHardwareBridge433MHzSenderPin, HttpApiController);

            // Setup the controller which creates ports for wireless sockets (433Mhz).
            var ic = new IntertechnoCodeSequenceProvider();
            var remoteSwitchController = new RemoteSocketController(new DeviceId("RemoteSocketController"), remoteSwitchSender, Timer)
                .WithRemoteSocket(0, ic.GetSequence(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1, RemoteSocketCommand.TurnOn), ic.GetSequence(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1, RemoteSocketCommand.TurnOff));
            
            // Setup the weather station which provides sunrise and sunset information.
            CreateWeatherStation();

            // Add new rooms with actuators here! Example:
            var exampleRoom = this.CreateArea(RoomId.ExampleRoom)
                .WithTemperatureSensor(ExampleRoom.TemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(5))
                .WithHumiditySensor(ExampleRoom.HumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(5))
                .WithMotionDetector(ExampleRoom.MotionDetector, hspe16.GetInput(8))
                .WithWindow(ExampleRoom.Window, w => w.WithCenterCasement(hspe16.GetInput(0)))
                .WithLamp(ExampleRoom.Lamp1, remoteSwitchController.GetOutput(0))
                .WithSocket(ExampleRoom.Socket1, hsrel5.GetOutput(0))
                .WithSocket(ExampleRoom.Socket2, hsrel5.GetOutput(4))
                .WithSocket(ExampleRoom.BathroomFan, hsrel5.GetOutput(3))
                .WithLamp(ExampleRoom.Lamp2, hsrel8.GetOutput(0))
                .WithLamp(ExampleRoom.Lamp3, hsrel8.GetOutput(1))
                .WithLamp(ExampleRoom.Lamp4, hsrel8.GetOutput(2))
                .WithLamp(ExampleRoom.Lamp5, hsrel8.GetOutput(3))
                .WithLamp(ExampleRoom.Lamp6, hsrel8.GetOutput(4))
                .WithButton(ExampleRoom.Button1, hspe16.GetInput(1))
                .WithButton(ExampleRoom.Button2, hspe16.GetInput(2))
                .WithVirtualButtonGroup(ExampleRoom.LedStripRemote, g => SetupLEDStripRemote(i2CHardwareBridge, g))
                .WithStateMachine(ExampleRoom.CeilingFan, (sm, r) => SetupCeilingFan(sm));
            
            exampleRoom.Lamp(ExampleRoom.Lamp5).ConnectToggleActionWith(exampleRoom.Button(ExampleRoom.Button1));
            exampleRoom.Lamp(ExampleRoom.Lamp6).ConnectToggleActionWith(exampleRoom.Button(ExampleRoom.Button1), ButtonPressedDuration.Long);
            exampleRoom.StateMachine(ExampleRoom.CeilingFan).ConnectMoveNextAndToggleOffWith(exampleRoom.Button(ExampleRoom.Button2));

            SetupHumidityDependingOutput(exampleRoom.HumiditySensor(ExampleRoom.HumiditySensor), hsrel8.GetOutput(5));

            exampleRoom.SetupTurnOnAndOffAutomation()
                .WithTrigger(exampleRoom.MotionDetector(ExampleRoom.MotionDetector))
                .WithTarget(exampleRoom.BinaryStateOutput(ExampleRoom.BathroomFan))
                .WithTarget(exampleRoom.BinaryStateOutput(ExampleRoom.Lamp2))
                .WithOnDuration(TimeSpan.FromSeconds(10));

            PublishStatisticsNotification();

            // Setup the CSV writer which writes all state changes to the SD card (package directory).
            var localCsvFileWriter = new CsvHistory(Logger, HttpApiController);
            localCsvFileWriter.ConnectActuators(this);

            Timer.Tick += (s, e) =>
            {
                pi2PortController.PollOpenInputPorts();
                ccToolsBoardController.PollInputBoardStates();
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

        private void SetupCeilingFan(StateMachine stateMachine)
        {
            var relayBoard = Device<HSREL5>(DeviceIdFactory.CreateIdFrom(Device.HSRel5));
            var gear1 = relayBoard.GetOutput(2);
            var gear2 = relayBoard.GetOutput(1);

            stateMachine.AddOffState().WithLowPort(gear1).WithLowPort(gear2);

            stateMachine.AddState("1").WithHighPort(gear1).WithLowPort(gear2);
            stateMachine.AddState("2").WithLowPort(gear1).WithHighPort(gear2);
        }

        private void SetupLEDStripRemote(I2CHardwareBridge i2CHardwareBridge, VirtualButtonGroup group)
        {
            var ledStripRemote = new LEDStripRemote(i2CHardwareBridge, 4);

            group.WithButton(new ActuatorId("on"), b => b.WithShortAction(() => ledStripRemote.TurnOn()))
                .WithButton(new ActuatorId("off"), b => b.WithShortAction(() => ledStripRemote.TurnOff()))
                .WithButton(new ActuatorId("white"), b => b.WithShortAction(() => ledStripRemote.TurnWhite()))

                .WithButton(new ActuatorId("red1"), b => b.WithShortAction(() => ledStripRemote.TurnRed1()))
                .WithButton(new ActuatorId("green1"), b => b.WithShortAction(() => ledStripRemote.TurnGreen1()))
                .WithButton(new ActuatorId("blue1"), b => b.WithShortAction(() => ledStripRemote.TurnBlue1()));
        }

        private void CreateWeatherStation()
        {
            try
            {
                var configuration = JsonObject.Parse(File.ReadAllText(Path.Combine(ApplicationData.Current.LocalFolder.Path, "WeatherStationConfiguration.json")));

                double lat = configuration.GetNamedNumber("lat");
                double lon = configuration.GetNamedNumber("lon");
                string appId = configuration.GetNamedString("appID");

                var weatherStation = new OWMWeatherStation(DeviceIdFactory.CreateIdFrom(Device.WeatherStation), lat, lon, appId, Timer, HttpApiController, Logger);
                Logger.Info("WeatherStation initialized successfully");

                AddDevice(weatherStation);
            }
            catch (Exception exception)
            {
                Logger.Warning("Unable to create weather station. " + exception.Message);
            }            
        }
    }
}
