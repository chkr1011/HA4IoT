using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Automations;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.DHT22;
using HA4IoT.Hardware.GenericIOBoard;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class UpperBathroomConfiguration
    {
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

        public void Setup(Home home, CCToolsBoardController ccToolsController, IOBoardManager ioBoardManager, DHT22Accessor dht22Accessor)
        {
            var hsrel5 = ccToolsController.CreateHSREL5(Device.UpperBathroomHSREL5, 61);
            var input5 = ioBoardManager.GetInputBoard(Device.Input5);

            const int SensorPin = 4; //7;

            var bathroom = home.AddRoom(Room.UpperBathroom)
                .WithTemperatureSensor(UpperBathroom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(UpperBathroom.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorPin))
                .WithMotionDetector(UpperBathroom.MotionDetector, input5.GetInput(15))
                .WithLamp(UpperBathroom.LightCeilingDoor, hsrel5.GetOutput(0))
                .WithLamp(UpperBathroom.LightCeilingEdge, hsrel5.GetOutput(1))
                .WithLamp(UpperBathroom.LightCeilingMirrorCabinet, hsrel5.GetOutput(2))
                .WithLamp(UpperBathroom.LampMirrorCabinet, hsrel5.GetOutput(3));

            var combinedLights =
                bathroom.CombineActuators(UpperBathroom.CombinedCeilingLights)
                    .WithActuator(bathroom.Lamp(UpperBathroom.LightCeilingDoor))
                    .WithActuator(bathroom.Lamp(UpperBathroom.LightCeilingEdge))
                    .WithActuator(bathroom.Lamp(UpperBathroom.LightCeilingMirrorCabinet))
                    .WithActuator(bathroom.Lamp(UpperBathroom.LampMirrorCabinet));

            bathroom.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(bathroom.MotionDetector(UpperBathroom.MotionDetector))
                .WithTarget(combinedLights)
                .WithOnDuration(TimeSpan.FromMinutes(8));

            var fanPort0 = hsrel5.GetOutput(4);
            var fanPort1 = hsrel5.GetOutput(5);
            var fan = bathroom.AddStateMachine(UpperBathroom.Fan);
            fan.AddOffState().WithPort(fanPort0, BinaryState.Low).WithPort(fanPort1, BinaryState.Low);
            fan.AddState("1").WithPort(fanPort0, BinaryState.High).WithPort(fanPort1, BinaryState.Low);
            fan.AddState("2").WithPort(fanPort0, BinaryState.High).WithPort(fanPort1, BinaryState.High);
            fan.TurnOff();

            new AutomaticBathroomFanAutomation(home.Timer)
                .WithSlowDuration(TimeSpan.FromMinutes(8))
                .WithFastDuration(TimeSpan.FromMinutes(12))
                .WithMotionDetector(bathroom.MotionDetector(UpperBathroom.MotionDetector))
                .WithActuator(fan);
        }
    }
}
