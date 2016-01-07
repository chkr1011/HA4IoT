using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Automations;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.DHT22;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class UpperBathroomConfiguration
    {
        private readonly Controller _controller;
        private readonly HSREL5 _hsrel5;
        private readonly IBinaryInputController _input5;

        private enum UpperBathroom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingDoor,
            LightCeilingEdge,
            LightCeilingMirrorCabinet,
            LampMirrorCabinet,

            Fan,

            CombinedCeilingLights
        }

        public UpperBathroomConfiguration(Controller controller, CCToolsBoardController ccToolsController)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (ccToolsController == null) throw new ArgumentNullException(nameof(ccToolsController));

            _controller = controller;

            _hsrel5 = ccToolsController.CreateHSREL5(Device.UpperBathroomHSREL5, new I2CSlaveAddress(61));
            _input5 = controller.GetDevice<HSPE16InputOnly>(Device.Input5);
        }

        public void Setup(DHT22Accessor dht22Accessor)
        {
            const int SensorPin = 4;

            var bathroom = _controller.CreateRoom(Room.UpperBathroom)
                .WithTemperatureSensor(UpperBathroom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(UpperBathroom.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorPin))
                .WithMotionDetector(UpperBathroom.MotionDetector, _input5.GetInput(15))
                .WithLamp(UpperBathroom.LightCeilingDoor, _hsrel5.GetOutput(0))
                .WithLamp(UpperBathroom.LightCeilingEdge, _hsrel5.GetOutput(1))
                .WithLamp(UpperBathroom.LightCeilingMirrorCabinet, _hsrel5.GetOutput(2))
                .WithLamp(UpperBathroom.LampMirrorCabinet, _hsrel5.GetOutput(3))
                .WithStateMachine(UpperBathroom.Fan, SetupFan);

            var combinedLights =
                bathroom.CombineActuators(UpperBathroom.CombinedCeilingLights)
                    .WithActuator(bathroom.Lamp(UpperBathroom.LightCeilingDoor))
                    .WithActuator(bathroom.Lamp(UpperBathroom.LightCeilingEdge))
                    .WithActuator(bathroom.Lamp(UpperBathroom.LightCeilingMirrorCabinet))
                    .WithActuator(bathroom.Lamp(UpperBathroom.LampMirrorCabinet));

            bathroom.SetupAutomaticTurnOnAndOffAutomation()
                .WithTrigger(bathroom.MotionDetector(UpperBathroom.MotionDetector))
                .WithTarget(combinedLights)
                .WithOnDuration(TimeSpan.FromMinutes(8));
            
            new AutomaticBathroomFanAutomation(_controller.Timer)
                .WithSlowDuration(TimeSpan.FromMinutes(8))
                .WithFastDuration(TimeSpan.FromMinutes(12))
                .WithMotiWithTrigger(bathroom.MotionDetector(UpperBathroom.MotionDetector))
                .WithActuator(bathroom.StateMachine(UpperBathroom.Fan));
        }

        private void SetupFan(StateMachine stateMachine, IRoom room)
        {
            var fanPort0 = _hsrel5.GetOutput(4);
            var fanPort1 = _hsrel5.GetOutput(5);

            stateMachine.AddOffState().WithPort(fanPort0, BinaryState.Low).WithPort(fanPort1, BinaryState.Low);
            stateMachine.AddState("1").WithPort(fanPort0, BinaryState.High).WithPort(fanPort1, BinaryState.Low);
            stateMachine.AddState("2").WithPort(fanPort0, BinaryState.High).WithPort(fanPort1, BinaryState.High);
            stateMachine.TurnOff();
        }
    }
}
