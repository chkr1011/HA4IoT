using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.DHT22;

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

        public void Setup(Home home, CCToolsBoardController ccToolsController, DHT22Reader sensorBridgeDriver)
        {
            var hspe16_FloorAndLowerBathroom = ccToolsController.GetOutputBoard(Device.LowerFloorAndLowerBathroomHSPE16);
            var input3 = ccToolsController.GetInputBoard(Device.Input3);

            const int SensorID = 5;

            var bathroom = home.AddRoom(Room.LowerBathroom)
                .WithMotionDetector(LowerBathroom.MotionDetector, input3.GetInput(15))
                .WithTemperatureSensor(LowerBathroom.TemperatureSensor, SensorID, sensorBridgeDriver)
                .WithHumiditySensor(LowerBathroom.HumditySensor, SensorID, sensorBridgeDriver)
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
