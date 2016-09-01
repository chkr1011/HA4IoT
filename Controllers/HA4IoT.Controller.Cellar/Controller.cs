using System;
using System.Threading.Tasks;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Core;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Services;
using HA4IoT.Services.Environment;

namespace HA4IoT.Controller.Cellar
{
    internal class Controller : ControllerBase
    {
        private enum Device
        {
            CellarHSRT16
        }

        private enum RoomId
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

        public Controller(int statusLedNumber)
            : base(statusLedNumber)
        {
        }

        protected override async Task ConfigureAsync(IDeviceService deviceService)
        {
            var pi2PortController = new Pi2GpioService();
            
            var openWeatherMapService = new OpenWeatherMapService(
                ServiceLocator.GetService<IDateTimeService>(),
                ServiceLocator.GetService<ISchedulerService>(),
                ServiceLocator.GetService<ISystemInformationService>());

            ServiceLocator.RegisterService(typeof(IOutdoorTemperatureService), new OutdoorTemperatureService(openWeatherMapService, ServiceLocator.GetService<IDateTimeService>()));
            ServiceLocator.RegisterService(typeof(IOutdoorHumidityService), new OutdootHumidityService(openWeatherMapService, ServiceLocator.GetService<IDateTimeService>()));
            ServiceLocator.RegisterService(typeof(IDaylightService), new DaylightService(openWeatherMapService, ServiceLocator.GetService<IDateTimeService>()));
            ServiceLocator.RegisterService(typeof(IWeatherService), new WeatherService(openWeatherMapService, ServiceLocator.GetService<IDateTimeService>()));
            ServiceLocator.RegisterService(typeof(OpenWeatherMapService), openWeatherMapService);

            var ccToolsFactory = new CCToolsBoardService(this, GetDevice<II2CBusService>());
            var hsrt16 = ccToolsFactory.CreateHSRT16(Device.CellarHSRT16, new I2CSlaveAddress(32));

            var garden = this.CreateArea(RoomId.Garden)
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

            garden.GetStateMachine(Garden.StateMachine).ConnectMoveNextAndToggleOffWith(garden.GetButton(Garden.Button));

            garden.SetupConditionalOnAutomation()
                .WithActuator(garden.GetLamp(Garden.LampParkingLot))
                .WithOnAtNightRange()
                .WithOffBetweenRange(TimeSpan.Parse("22:30:00"), TimeSpan.Parse("05:00:00"));

            TimerService.Tick += (s, e) => { pi2PortController.PollOpenInputPorts(); };

            await base.ConfigureAsync();
        }

        private void SetupStateMachine(StateMachine stateMachine, IArea garden)
        {
            stateMachine.AddOffState()
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new NamedComponentState("Te"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new NamedComponentState("G"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new NamedComponentState("W"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new NamedComponentState("D"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new NamedComponentState("Ti"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.On);

            stateMachine.AddState(new NamedComponentState("G+W"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new NamedComponentState("Te+G+W"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new NamedComponentState("Te+G+W+D+Ti"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.On);
        }
    }
}
