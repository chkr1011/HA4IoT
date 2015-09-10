using System;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Automations;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.DHT22;

namespace CK.HomeAutomation.Controller.Rooms
{
    internal class UpperBathroomConfiguration
    {
        public void Setup(Home home, CCToolsBoardController ccToolsController, DHT22Reader sensorBridgeDriver)
        {
            var hsrel5 = ccToolsController.CreateHSREL5(Device.UpperBathroomHSREL5, 61);
            var input5 = ccToolsController.GetInputBoard(Device.Input5);

            var bathroom = home.AddRoom(Room.UpperBathroom)
                .WithTemperatureSensor(UpperBathroom.TemperatureSensor, 7, sensorBridgeDriver)
                .WithHumiditySensor(UpperBathroom.HumiditySensor, 7, sensorBridgeDriver)
                .WithMotionDetector(UpperBathroom.MotionDetector, input5.GetInput(15))
                .WithLamp(UpperBathroom.LightCeilingDoor, hsrel5.GetOutput(0))
                .WithLamp(UpperBathroom.LightCeilingEdge, hsrel5.GetOutput(1))
                .WithLamp(UpperBathroom.LightCeilingMirrorCabinet, hsrel5.GetOutput(2))
                .WithLamp(UpperBathroom.LampMirrorCabinet, hsrel5.GetOutput(3));

            var combinedLights =
                bathroom.CombineActuators(UpperBathroom.CombinedCeilingLights)
                    .WithMaster(bathroom.Lamp(UpperBathroom.LightCeilingDoor))
                    .WithActuator(bathroom.Lamp(UpperBathroom.LightCeilingEdge))
                    .WithActuator(bathroom.Lamp(UpperBathroom.LightCeilingMirrorCabinet))
                    .WithActuator(bathroom.Lamp(UpperBathroom.LampMirrorCabinet));

            bathroom.SetupAutomaticTurnOnAction()
                .WithMotionDetector(bathroom.MotionDetector(UpperBathroom.MotionDetector))
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
    }
}
