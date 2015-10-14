using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Connectors;
using CK.HomeAutomation.Actuators.Contracts;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.GenericIOBoard;
using CK.HomeAutomation.Hardware.OpenWeatherMapWeatherStation;
using CK.HomeAutomation.Hardware.Pi2;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Controller.Cellar
{
    internal class Controller : BaseController
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

            LampParkingLot1,
            LampParkingLot2,
            LampParkingLot3,
            CombinedParkingLotLamps,

            SocketPavillion,

            StateMachine
        }

        protected override void Initialize()
        {
            InitializeHealthMonitor(22);

            var pi2PortController = new Pi2PortController();

            IWeatherStation weatherStation = CreateWeatherStation();
            var i2CBus = new I2cBusAccessor(NotificationHandler);

            var ioBoardManager = new IOBoardManager(HttpApiController, NotificationHandler);
            var ccToolsFactory = new CCToolsBoardController(i2CBus, ioBoardManager, NotificationHandler);
            var hsrt16 = ccToolsFactory.CreateHSRT16(Device.CellarHSRT16, 32);

            var home = new Home(Timer, HealthMonitor, weatherStation, HttpApiController, NotificationHandler);

            var garden = home.AddRoom(Room.Garden)
                .WithLamp(Garden.LampTerrace, hsrt16.GetOutput(15))
                .WithLamp(Garden.LampGarage, hsrt16.GetOutput(14))
                .WithLamp(Garden.LampTap, hsrt16.GetOutput(13))
                .WithLamp(Garden.SpotlightRoof, hsrt16.GetOutput(12))
                .WithLamp(Garden.LampRearArea, hsrt16.GetOutput(11))
                .WithSocket(Garden.SocketPavillion, hsrt16.GetOutput(10))
                // 9 = free
                .WithLamp(Garden.LampParkingLot1, hsrt16.GetOutput(8))
                .WithLamp(Garden.LampParkingLot2, hsrt16.GetOutput(6))
                .WithLamp(Garden.LampParkingLot3, hsrt16.GetOutput(7))

                .WithButton(Garden.Button, pi2PortController.GetInput(4).WithInvertedState());

            var stateMachine = garden.AddStateMachine(Garden.StateMachine);

            stateMachine.AddOffState()
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState()
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState()
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState()
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState()
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState()
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.On);

            stateMachine.AddState()
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState()
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.Off)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.Off);

            stateMachine.AddState()
                .WithActuator(garden.Lamp(Garden.LampTerrace), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampGarage), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampTap), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.SpotlightRoof), BinaryActuatorState.On)
                .WithActuator(garden.Lamp(Garden.LampRearArea), BinaryActuatorState.On);

            garden.StateMachine(Garden.StateMachine).ConnectMoveNextAndToggleOffWith(garden.Button(Garden.Button));

            garden.CombineActuators(Garden.CombinedParkingLotLamps)
                .WithActuator(garden.Lamp(Garden.LampParkingLot1))
                .WithActuator(garden.Lamp(Garden.LampParkingLot2)) // Mitte
                .WithActuator(garden.Lamp(Garden.LampParkingLot3));

            garden.SetupAlwaysOn()
                .WithActuator(garden.BinaryStateOutput(Garden.CombinedParkingLotLamps))
                .WithOnlyAtNightRange(home.WeatherStation)
                .WithOffBetweenRange(TimeSpan.FromHours(22).Add(TimeSpan.FromMinutes(30)), TimeSpan.FromHours(5));

            home.PublishStatisticsNotification();

            Timer.Tick += (s, e) => { pi2PortController.PollOpenInputPorts(); };
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
