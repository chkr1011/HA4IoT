using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.GenericIOBoard;
using HA4IoT.Hardware.OpenWeatherMapWeatherStation;
using HA4IoT.Hardware.Pi2;

namespace HA4IoT.Controller.Cellar
{
    internal class Controller : ControllerBase
    {
        private enum Device
        {
            CellarHSRT16
        }

        private enum Room
        {
            Garden
        }

        private enum Garden
        {
            Button,

            LampTerrace,
            LampTap,
            LampGarage,
            SpotlightRoof,
            LampRearArea,
            LampParkingLot,

            SocketPavillion,

            StateMachine
        }

        protected override void Initialize()
        {
            InitializeHealthMonitor(22);

            var pi2PortController = new Pi2PortController();

            IWeatherStation weatherStation = CreateWeatherStation();
            var i2CBus = new I2CBusWrapper(NotificationHandler);

            var ioBoardManager = new IOBoardCollection(HttpApiController, NotificationHandler);
            var ccToolsFactory = new CCToolsBoardController(i2CBus, ioBoardManager, NotificationHandler);
            var hsrt16 = ccToolsFactory.CreateHSRT16(Device.CellarHSRT16, new I2CSlaveAddress(32));

            var home = new Home(Timer, HealthMonitor, weatherStation, HttpApiController, NotificationHandler);

            var garden = home.AddRoom(Room.Garden)
                .WithLamp(Garden.LampTerrace, hsrt16[HSRT16Pin.Relay15])
                .WithLamp(Garden.LampGarage, hsrt16[HSRT16Pin.Relay14])
                .WithLamp(Garden.LampTap, hsrt16[HSRT16Pin.Relay13])
                .WithLamp(Garden.SpotlightRoof, hsrt16[HSRT16Pin.Relay12])
                .WithLamp(Garden.LampRearArea, hsrt16[HSRT16Pin.Relay11])
                .WithSocket(Garden.SocketPavillion, hsrt16[HSRT16Pin.Relay10])
                // 9 = free
                .WithLamp(Garden.LampParkingLot, new LogicalBinaryOutput().WithOutput(hsrt16[HSRT16Pin.Relay8]).WithOutput(hsrt16[HSRT16Pin.Relay6]).WithOutput(hsrt16[HSRT16Pin.Relay7]))
                .WithButton(Garden.Button, pi2PortController.GetInput(4).WithInvertedState())
                .WithStateMachine(Garden.StateMachine, SetupStateMachine);
            
            garden.StateMachine(Garden.StateMachine).ConnectMoveNextAndToggleOffWith(garden.Button(Garden.Button));

            garden.SetupAlwaysOn()
                .WithActuator(garden.Lamp(Garden.LampParkingLot))
                .WithOnAtNightRange(home.WeatherStation)
                .WithOffBetweenRange(TimeSpan.Parse("22:30:00"), TimeSpan.Parse("05:00:00"));

            home.PublishStatisticsNotification();

            Timer.Tick += (s, e) => { pi2PortController.PollOpenInputPorts(); };
        }

        private void SetupStateMachine(StateMachine stateMachine, Actuators.Room garden)
        {
            stateMachine.AddOffState()
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState("Te")
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState("G")
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState("W")
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState("D")
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState("Ti")
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.On);

            stateMachine.AddState("G+W")
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState("Te+G+W")
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState("Te+G+W+D+Ti")
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.On);
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
