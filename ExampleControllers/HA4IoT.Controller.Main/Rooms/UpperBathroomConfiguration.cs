using System;
using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;

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
            _input5 = controller.Device<HSPE16InputOnly>(Device.Input5);
        }

        public void Setup()
        {
            const int SensorPin = 4;

            var i2cHardwareBridge = _controller.GetDevice<I2CHardwareBridge>();

            var bathroom = _controller.CreateArea(Room.UpperBathroom)
                .WithTemperatureSensor(UpperBathroom.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(UpperBathroom.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithMotionDetector(UpperBathroom.MotionDetector, _input5.GetInput(15))
                .WithLamp(UpperBathroom.LightCeilingDoor, _hsrel5.GetOutput(0))
                .WithLamp(UpperBathroom.LightCeilingEdge, _hsrel5.GetOutput(1))
                .WithLamp(UpperBathroom.LightCeilingMirrorCabinet, _hsrel5.GetOutput(2))
                .WithLamp(UpperBathroom.LampMirrorCabinet, _hsrel5.GetOutput(3))
                .WithStateMachine(UpperBathroom.Fan, SetupFan);

            var combinedLights =
                bathroom.CombineActuators(UpperBathroom.CombinedCeilingLights)
                    .WithActuator(bathroom.GetLamp(UpperBathroom.LightCeilingDoor))
                    .WithActuator(bathroom.GetLamp(UpperBathroom.LightCeilingEdge))
                    .WithActuator(bathroom.GetLamp(UpperBathroom.LightCeilingMirrorCabinet))
                    .WithActuator(bathroom.GetLamp(UpperBathroom.LampMirrorCabinet));

            bathroom.SetupTurnOnAndOffAutomation()
                .WithTrigger(bathroom.GetMotionDetector(UpperBathroom.MotionDetector))
                .WithTarget(combinedLights)
                .WithOnDuration(TimeSpan.FromMinutes(8));
            
            new BathroomFanAutomation(AutomationIdFactory.CreateIdFrom<BathroomFanAutomation>(bathroom), _controller.Timer)
                .WithTrigger(bathroom.GetMotionDetector(UpperBathroom.MotionDetector))
                .WithSlowDuration(TimeSpan.FromMinutes(8))
                .WithFastDuration(TimeSpan.FromMinutes(12))
                .WithActuator(bathroom.GetStateMachine(UpperBathroom.Fan));
        }

        private void SetupFan(StateMachine stateMachine, IArea room)
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
