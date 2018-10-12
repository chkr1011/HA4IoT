using System;
using System.Text;
using System.Threading.Tasks;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Areas;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Commands;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.RaspberryPi;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Hardware;
using HA4IoT.Hardware.Drivers.CCTools.Devices;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;

namespace HA4IoT.Controller.Main.Cellar
{
    internal sealed class Configuration : IConfiguration
    {
        private readonly IDeviceRegistryService _deviceRegistryService;
        private readonly IGpioService _gpioService;
        private readonly IAreaRegistryService _areaService;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IMessageBrokerService _messageBroker;
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;

        private IArea _area;

        public Configuration(
            IDeviceRegistryService deviceRegistryService,
            IGpioService gpioService,
            IAreaRegistryService areaService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            AutomationFactory automationFactory,
            IMessageBrokerService messageBroker,
            IDeviceMessageBrokerService deviceMessageBrokerService)
        {
            _deviceRegistryService = deviceRegistryService ?? throw new ArgumentNullException(nameof(deviceRegistryService));
            _gpioService = gpioService ?? throw new ArgumentNullException(nameof(gpioService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _deviceMessageBrokerService = deviceMessageBrokerService ?? throw new ArgumentNullException(nameof(deviceMessageBrokerService));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
        }

        public Task ApplyAsync()
        {
            var hsrt16 = _deviceRegistryService.GetDevice<HSRT16>("HSRT16");

            var garden = _areaService.RegisterArea("Garden");
            _area = garden;

            var parkingLotOutput = new LogicalBinaryOutput(hsrt16[HSRT16Pin.Relay6], hsrt16[HSRT16Pin.Relay7], hsrt16[HSRT16Pin.Relay8]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampParkingLot, parkingLotOutput).StateChanged += OnStateChanged;
            // Relay 9 is free.
            _actuatorFactory.RegisterSocket(garden, Garden.SocketPavillion, hsrt16[HSRT16Pin.Relay10]).StateChanged += OnStateChanged;
            _actuatorFactory.RegisterLamp(garden, Garden.LampRearArea, hsrt16[HSRT16Pin.Relay11]).StateChanged += OnStateChanged;
            _actuatorFactory.RegisterLamp(garden, Garden.SpotlightRoof, hsrt16[HSRT16Pin.Relay12]).StateChanged += OnStateChanged;
            _actuatorFactory.RegisterLamp(garden, Garden.LampTap, hsrt16[HSRT16Pin.Relay13]).StateChanged += OnStateChanged;
            _actuatorFactory.RegisterLamp(garden, Garden.LampGarage, hsrt16[HSRT16Pin.Relay14]).StateChanged += OnStateChanged;
            _actuatorFactory.RegisterLamp(garden, Garden.LampTerrace, hsrt16[HSRT16Pin.Relay15]).StateChanged += OnStateChanged;

            var stateMachine = _actuatorFactory.RegisterStateMachine(garden, Garden.StateMachine, InitializeStateMachine);

            var button = _sensorFactory.RegisterButton(garden, Garden.Button, _gpioService.GetInput(4, GpioPullMode.High, GpioInputMonitoringMode.Interrupt).WithInvertedState());
            button.CreatePressedShortTrigger(_messageBroker).Attach(() => stateMachine.TrySetNextState());
            button.CreatePressedLongTrigger(_messageBroker).Attach(() => stateMachine.TryTurnOff());
            button.StateChanged += OnStateChanged;

            _automationFactory.RegisterConditionalOnAutomation(garden, Garden.LampParkingLotAutomation)
                .WithComponent(garden.GetLamp(Garden.LampParkingLot))
                .WithOnAtNightRange()
                .WithOffBetweenRange(TimeSpan.Parse("22:30:00"), TimeSpan.Parse("05:00:00"));

            _deviceMessageBrokerService.MessageReceived += OnMessageReceived;

            return Task.FromResult(0);
        }

        private void OnStateChanged(object sender, ComponentFeatureStateChangedEventArgs e)
        {
            try
            {
                var topic = "garden_controller/$STATUS/";

                string payload;
                if (e.NewState.Supports<ButtonState>())
                {
                    topic += "button.default";
                    payload = e.NewState.Extract<ButtonState>().Value == ButtonStateValue.Pressed ? "pressed" : "released";
                }
                else if (e.NewState.Supports<PowerState>())
                {
                    var powerState = e.NewState.Extract<PowerState>();
                    payload = powerState.Value == PowerStateValue.On ? "on" : "off";
                }
                else
                {
                    return;
                }

                var message = new DeviceMessage
                {
                    Topic = topic,
                    Payload = Encoding.ASCII.GetBytes(payload),
                    Retain = true
                };

                _deviceMessageBrokerService.Publish(message);
            }
            catch (Exception exception)
            {
                Log.Default.Error(exception, "Errow while sending state.");
            }
        }

        private void OnMessageReceived(object sender, DeviceMessageReceivedEventArgs e)
        {
            try
            {
                IComponent component = null;
                if (e.Message.Topic == "garden_controller/$PATCH/socket.pavillion")
                {
                    component = _area.GetComponent(Garden.SocketPavillion);
                }
                else if (e.Message.Topic == "garden_controller/$PATCH/lamp.rear_area")
                {
                    component = _area.GetComponent(Garden.LampRearArea);
                }
                else if (e.Message.Topic == "garden_controller/$PATCH/lamp.roof")
                {
                    component = _area.GetComponent(Garden.SpotlightRoof);
                }
                else if (e.Message.Topic == "garden_controller/$PATCH/lamp.tap")
                {
                    component = _area.GetComponent(Garden.LampTap);
                }
                else if (e.Message.Topic == "garden_controller/$PATCH/lamp.garage")
                {
                    component = _area.GetComponent(Garden.LampGarage);
                }
                else if (e.Message.Topic == "garden_controller/$PATCH/lamp.terrace")
                {
                    component = _area.GetComponent(Garden.LampTerrace);
                }
                else if (e.Message.Topic == "garden_controller/$PATCH/lamp.parking_lot")
                {
                    component = _area.GetComponent(Garden.LampParkingLot);
                }

                if (component == null)
                {
                    return;
                }

                var status = Encoding.UTF8.GetString(e.Message.Payload ?? new byte[0]);

                ICommand command = new TurnOffCommand();

                if (status == "on")
                {
                    command = new TurnOnCommand();
                }
                else if (status == "toggle")
                {
                    command = new TogglePowerStateCommand();
                }

                component.ExecuteCommand(command);
            }
            catch (Exception exception)
            {
                Log.Default.Error(exception, "Errow while patching state.");
            }
        }

        private void InitializeStateMachine(StateMachine stateMachine, IArea garden)
        {
            var turnOffCommand = new TurnOffCommand();
            var turnOnCommand = new TurnOnCommand();

            stateMachine.ResetStateId = StateMachineStateExtensions.OffStateId;

            stateMachine.AddOffState()
                .WithCommand(garden.GetComponent(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("Te")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("G")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("W")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("D")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("Ti")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOnCommand);

            stateMachine.AddState("G+W")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("Te+G+W")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddOnState()
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOnCommand);
        }
    }
}
