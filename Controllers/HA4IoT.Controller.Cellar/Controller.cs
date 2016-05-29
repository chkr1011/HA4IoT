using System;
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
using HA4IoT.Core;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Sensors.Buttons;

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

        protected override void Initialize()
        {
            InitializeHealthMonitor(22);

            var pi2PortController = new Pi2PortController();

            AddDevice(new BuiltInI2CBus());

            RegisterService(new OpenWeatherMapWeatherService(Timer, ApiController));

            var ccToolsFactory = new CCToolsBoardController(this, GetDevice<II2CBus>());
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
                .WithOnAtNightRange(GetService<IDaylightService>())
                .WithOffBetweenRange(TimeSpan.Parse("22:30:00"), TimeSpan.Parse("05:00:00"));

            Timer.Tick += (s, e) => { pi2PortController.PollOpenInputPorts(); };
        }

        private void SetupStateMachine(StateMachine stateMachine, IArea garden)
        {
            stateMachine.AddOffState()
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new StatefulComponentState("Te"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new StatefulComponentState("G"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new StatefulComponentState("W"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new StatefulComponentState("D"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new StatefulComponentState("Ti"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.On);

            stateMachine.AddState(new StatefulComponentState("G+W"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new StatefulComponentState("Te+G+W"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.Off)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.Off);

            stateMachine.AddState(new StatefulComponentState("Te+G+W+D+Ti"))
                .WithActuator(garden.GetLamp(Garden.LampTerrace), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampGarage), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampTap), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.SpotlightRoof), BinaryStateId.On)
                .WithActuator(garden.GetLamp(Garden.LampRearArea), BinaryStateId.On);
        }
    }
}
