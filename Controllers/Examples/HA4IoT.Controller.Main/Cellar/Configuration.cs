using System;
using System.Threading.Tasks;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Services.Areas;

namespace HA4IoT.Controller.Main.Cellar
{
    internal sealed class Configuration : IConfiguration
    {
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly IGpioService _pi2GpioService;
        private readonly IAreaRegistryService _areaService;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly ILogService _logService;

        public Configuration(
            CCToolsDeviceService ccToolsBoardService,
            IGpioService pi2GpioService,
            IAreaRegistryService areaService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            AutomationFactory automationFactory,
            ILogService logService)
        {
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (pi2GpioService == null) throw new ArgumentNullException(nameof(pi2GpioService));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));

            _ccToolsBoardService = ccToolsBoardService;
            _pi2GpioService = pi2GpioService;
            _areaService = areaService;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
            _automationFactory = automationFactory;
            _logService = logService;
        }

        public Task ApplyAsync()
        {
            var hsrt16 = _ccToolsBoardService.RegisterHSRT16(InstalledDevice.CellarHSRT16.ToString(), new I2CSlaveAddress(32));

            var garden = _areaService.RegisterArea(Room.Garden);

            var parkingLotLamp = new LogicalBinaryOutput(hsrt16[HSRT16Pin.Relay6], hsrt16[HSRT16Pin.Relay7], hsrt16[HSRT16Pin.Relay8]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampParkingLot, parkingLotLamp);
            // Relay 9 is free.
            _actuatorFactory.RegisterSocket(garden, Garden.SocketPavillion, hsrt16[HSRT16Pin.Relay10]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampRearArea, hsrt16[HSRT16Pin.Relay11]);
            _actuatorFactory.RegisterLamp(garden, Garden.SpotlightRoof, hsrt16[HSRT16Pin.Relay12]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampTap, hsrt16[HSRT16Pin.Relay13]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampGarage, hsrt16[HSRT16Pin.Relay14]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampTerrace, hsrt16[HSRT16Pin.Relay15]);
            //_actuatorFactory.RegisterStateMachine(garden, Garden.StateMachine, InitializeStateMachine);

            _sensorFactory.RegisterButton(garden, Garden.Button, _pi2GpioService.GetInput(4).WithInvertedState());

            garden.GetStateMachine(Garden.StateMachine).ConnectMoveNextAndToggleOffWith(garden.GetButton(Garden.Button));

            _automationFactory.RegisterConditionalOnAutomation(garden, Garden.LampParkingLotAutomation)
                .WithComponent(garden.GetLamp(Garden.LampParkingLot))
                .WithOnAtNightRange()
                .WithOffBetweenRange(TimeSpan.Parse("22:30:00"), TimeSpan.Parse("05:00:00"));

            var ioBoardsInterruptMonitor = new InterruptMonitor(_pi2GpioService.GetInput(4), _logService);
            ioBoardsInterruptMonitor.InterruptDetected += (s, e) => _ccToolsBoardService.PollInputBoardStates();
            ioBoardsInterruptMonitor.Start();

            return Task.FromResult(0);
        }

        ////private void InitializeStateMachine(StateMachine stateMachine, IArea garden)
        ////{
        ////    stateMachine.AddOffState()
        ////        .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

        ////    stateMachine.AddState(new GenericComponentState("Te"))
        ////        .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

        ////    stateMachine.AddState(new GenericComponentState("G"))
        ////        .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

        ////    stateMachine.AddState(new GenericComponentState("W"))
        ////        .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

        ////    stateMachine.AddState(new GenericComponentState("D"))
        ////        .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

        ////    stateMachine.AddState(new GenericComponentState("Ti"))
        ////        .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.On);

        ////    stateMachine.AddState(new GenericComponentState("G+W"))
        ////        .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

        ////    stateMachine.AddState(new GenericComponentState("Te+G+W"))
        ////        .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
        ////        .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

        ////    stateMachine.AddState(new GenericComponentState("Te+G+W+D+Ti"))
        ////        .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.On)
        ////        .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.On);
        ////}
    }
}
