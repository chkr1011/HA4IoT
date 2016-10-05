using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Services.Areas;

namespace HA4IoT.Controller.Cellar
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const int LedGpio = 22;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var options = new ControllerOptions
            {
                StatusLedNumber = LedGpio,
                ConfigurationType = typeof(Configuration)
            };

            var controller = new Core.Controller(options);
            controller.RunAsync(taskInstance);
        }

        private class Configuration : IConfiguration
        {
            private readonly CCToolsBoardService _ccToolsBoardService;
            private readonly IPi2GpioService _pi2GpioService;
            private readonly SynonymService _synonymService;
            private readonly IAreaService _areaService;
            private readonly ActuatorFactory _actuatorFactory;
            private readonly SensorFactory _sensorFactory;
            private readonly AutomationFactory _automationFactory;

            private enum InstalledDevice
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
                LampParkingLotAutomation,

                SocketPavillion,

                StateMachine
            }

            public Configuration(
                CCToolsBoardService ccToolsBoardService, 
                IPi2GpioService pi2GpioService, 
                SynonymService synonymService, 
                IAreaService areaService,
                ActuatorFactory actuatorFactory,
                SensorFactory sensorFactory,
                AutomationFactory automationFactory)
            {
                if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
                if (pi2GpioService == null) throw new ArgumentNullException(nameof(pi2GpioService));
                if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
                if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
                if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));
                if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));

                _ccToolsBoardService = ccToolsBoardService;
                _pi2GpioService = pi2GpioService;
                _synonymService = synonymService;
                _areaService = areaService;
                _actuatorFactory = actuatorFactory;
                _sensorFactory = sensorFactory;
                _automationFactory = automationFactory;
            }
            
            public Task ApplyAsync()
            {
                var hsrt16 = _ccToolsBoardService.RegisterHSRT16(InstalledDevice.CellarHSRT16, new I2CSlaveAddress(32));

                var garden = _areaService.CreateArea(Room.Garden);

                var parkingLotLamp = new LogicalBinaryOutput(hsrt16[HSRT16Pin.Relay6], hsrt16[HSRT16Pin.Relay7], hsrt16[HSRT16Pin.Relay8]);
                _actuatorFactory.RegisterLamp(garden, Garden.LampParkingLot, parkingLotLamp);
                // Relay 9 is free.
                _actuatorFactory.RegisterSocket(garden, Garden.SocketPavillion, hsrt16[HSRT16Pin.Relay10]);
                _actuatorFactory.RegisterLamp(garden, Garden.LampRearArea, hsrt16[HSRT16Pin.Relay11]);
                _actuatorFactory.RegisterLamp(garden, Garden.SpotlightRoof, hsrt16[HSRT16Pin.Relay12]);
                _actuatorFactory.RegisterLamp(garden, Garden.LampTap, hsrt16[HSRT16Pin.Relay13]);
                _actuatorFactory.RegisterLamp(garden, Garden.LampGarage, hsrt16[HSRT16Pin.Relay14]);
                _actuatorFactory.RegisterLamp(garden, Garden.LampTerrace, hsrt16[HSRT16Pin.Relay15]);
                _actuatorFactory.RegisterStateMachine(garden, Garden.StateMachine, InitializeStateMachine);

                _sensorFactory.RegisterButton(garden, Garden.Button, _pi2GpioService.GetInput(4).WithInvertedState());

                garden.GetStateMachine(Garden.StateMachine).ConnectMoveNextAndToggleOffWith(garden.GetButton(Garden.Button));

                _automationFactory.RegisterConditionalOnAutomation(garden, Garden.LampParkingLotAutomation)
                    .WithActuator(garden.GetLamp(Garden.LampParkingLot))
                    .WithOnAtNightRange()
                    .WithOffBetweenRange(TimeSpan.Parse("22:30:00"), TimeSpan.Parse("05:00:00"));

                var ioBoardsInterruptMonitor = new InterruptMonitor(_pi2GpioService.GetInput(4));
                ioBoardsInterruptMonitor.InterruptDetected += (s, e) => _ccToolsBoardService.PollInputBoardStates();
                ioBoardsInterruptMonitor.Start();

                return Task.FromResult(0);
            }

            private void InitializeStateMachine(StateMachine stateMachine, IArea garden)
            {
                stateMachine.AddOffState()
                    .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

                stateMachine.AddState(new ComponentState("Te"))
                    .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

                stateMachine.AddState(new ComponentState("G"))
                    .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

                stateMachine.AddState(new ComponentState("W"))
                    .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

                stateMachine.AddState(new ComponentState("D"))
                    .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

                stateMachine.AddState(new ComponentState("Ti"))
                    .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.On);

                stateMachine.AddState(new ComponentState("G+W"))
                    .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

                stateMachine.AddState(new ComponentState("Te+G+W"))
                    .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                    .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

                stateMachine.AddState(new ComponentState("Te+G+W+D+Ti"))
                    .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.On)
                    .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.On);
            }
        }
    }
}