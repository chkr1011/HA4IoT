using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Storage;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Connectors;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.Pi2;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Controller.Cellar
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void Run()
        {
            var notificationHandler = new NotificationHandler();
            notificationHandler.Publish(NotificationType.Info, "Starting");

            var timer = new HomeAutomationTimer(notificationHandler);

            var httpServer = new HttpServer();
            httpServer.StartAsync(80).Wait();
            var httpApiController = new HttpRequestDispatcher(httpServer).GetController("api");

            var pi2PortManager = new Pi2PortManager();

            var healthMonitor = new HealthMonitor(pi2PortManager.GetOutput(22).WithInvertedState(), timer, httpApiController);

            WeatherStation weatherStation = CreateWeatherStation(timer, httpApiController, notificationHandler);
            var i2CBus = new I2CBus(notificationHandler);

            var ioBoardManager = new IOBoardManager(httpApiController, notificationHandler);
            var ccToolsFactory = new CCToolsFactory(i2CBus, ioBoardManager, notificationHandler);
            var hsrt16 = ccToolsFactory.CreateHSRT16(Device.CellarHSRT16, 32);

            var home = new Home(timer, healthMonitor, weatherStation, httpApiController, notificationHandler);

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
                
                .WithButton(Garden.Button, pi2PortManager.GetInput(4).WithInvertedState());

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

            garden.StateMachine(Garden.StateMachine).ConnectMoveNextWith(garden.Button(Garden.Button));

            garden.CombineActuators(Garden.CombinedParkingLotLamps)
                .WithActuator(garden.Lamp(Garden.LampParkingLot1))
                .WithActuator(garden.Lamp(Garden.LampParkingLot2)) // Mitte
                .WithActuator(garden.Lamp(Garden.LampParkingLot3));

            garden.SetupAlwaysOn()
                .WithActuator(garden.BinaryStateOutputActuator(Garden.CombinedParkingLotLamps))
                .WithOnlyAtNightRange(home.WeatherStation)
                .WithOffBetweenRange(TimeSpan.FromHours(22).Add(TimeSpan.FromMinutes(30)), TimeSpan.FromHours(5));

            home.PublishStatisticsNotification();
            
            timer.Tick += (s, e) => { pi2PortManager.PollOpenInputPorts(); };
            timer.Run();
        }

        private WeatherStation CreateWeatherStation(HomeAutomationTimer timer, HttpRequestController httpApiController, INotificationHandler notificationHandler)
        {
            try
            {
                var configuration = JsonObject.Parse(File.ReadAllText(Path.Combine(ApplicationData.Current.LocalFolder.Path, "WeatherStationConfiguration.json")));

                double lat = configuration.GetNamedNumber("lat");
                double lon = configuration.GetNamedNumber("lon");

                var weatherStation = new WeatherStation(lat, lon, timer, httpApiController, notificationHandler);
                notificationHandler.PublishFrom(this, NotificationType.Info, "WeatherStation initialized successfully.");
                return weatherStation;
            }
            catch (Exception exception)
            {
                notificationHandler.PublishFrom(this, NotificationType.Warning, "Unable to create weather station. " + exception.Message);
            }

            return null;
        }

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
    }
}