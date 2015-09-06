using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.Drivers;

namespace CK.HomeAutomation.Controller.Rooms
{
    internal class LowerBathroomConfiguration
    {
        public enum LowerBathroom
        {
            TemperatureSensor,
            HumditySensor,
            MotionDetector,

            LightCeilingDoor,
            LightCeilingMiddle,
            LightCeilingWindow,
            // Another light is available!
            CombinedLights,

            LampMirror,
        }

        public void Setup(Home home, IOBoardManager ioBoardManager, TemperatureAndHumiditySensorBridgeDriver sensorBridgeDriver)
        {
            var hspe16_FloorAndLowerBathroom = ioBoardManager.GetOutputBoard(Device.LowerFloorAndLowerBathroomHSPE16);
            var input3 = ioBoardManager.GetInputBoard(Device.Input3);

            var bathroom = home.AddRoom(Room.LowerBathroom)
                .WithMotionDetector(LowerBathroom.MotionDetector, input3.GetInput(15))
                .WithTemperatureSensor(LowerBathroom.TemperatureSensor, 5, sensorBridgeDriver)
                .WithHumiditySensor(LowerBathroom.HumditySensor, 5, sensorBridgeDriver)
                .WithLamp(LowerBathroom.LightCeilingDoor, hspe16_FloorAndLowerBathroom.GetOutput(0).WithInvertedState())
                .WithLamp(LowerBathroom.LightCeilingMiddle, hspe16_FloorAndLowerBathroom.GetOutput(1).WithInvertedState())
                .WithLamp(LowerBathroom.LightCeilingWindow, hspe16_FloorAndLowerBathroom.GetOutput(2).WithInvertedState())
                .WithLamp(LowerBathroom.LampMirror, hspe16_FloorAndLowerBathroom.GetOutput(4).WithInvertedState());

            bathroom.CombineActuators(LowerBathroom.CombinedLights)
                .WithActuator(bathroom.Lamp(LowerBathroom.LightCeilingDoor))
                .WithActuator(bathroom.Lamp(LowerBathroom.LightCeilingMiddle))
                .WithActuator(bathroom.Lamp(LowerBathroom.LightCeilingWindow))
                .WithActuator(bathroom.Lamp(LowerBathroom.LampMirror));

            bathroom.SetupAutomaticTurnOnAction()
                .WithMotionDetector(bathroom.MotionDetector(LowerBathroom.MotionDetector))
                .WithTarget(bathroom.BinaryStateOutput(LowerBathroom.CombinedLights));
        }
    }
}
