using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Animations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.DHT22;
using HA4IoT.Hardware.GenericIOBoard;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class FloorConfiguration
    {
        private enum Floor
        {
            StairwayMotionDetector,

            StairwayLampWall,
            StairwayLampCeiling,
            CombinedStairwayLamp,

            StairwayRollerShutter,

            LowerFloorTemperatureSensor,
            LowerFloorHumiditySensor,
            LowerFloorMotionDetector,

            StairsLowerMotionDetector,
            StairsUpperMotionDetector,

            ButtonLowerFloorUpper,
            ButtonLowerFloorLower,
            ButtonLowerFloorAtBathroom,
            ButtonLowerFloorAtKitchen,
            ButtonStairway,
            ButtonStairsLowerUpper,
            ButtonStairsLowerLower,
            ButtonStairsUpper,

            Lamp1,
            Lamp2,
            Lamp3,
            CombinedLamps,

            LampStairsCeiling1,
            LampStairsCeiling2,
            LampStairsCeiling3,
            LampStairsCeiling4,
            CombinedLampStairsCeiling,

            LampStairs1,
            LampStairs2,
            LampStairs3,
            LampStairs4,
            LampStairs5,
            LampStairs6,
            CombinedLampStairs
        }

        public void Setup(Home home, CCToolsBoardController ccToolsController, IOBoardManager ioBoardManager, DHT22Accessor dht22Accessor)
        {
            var hsrel5Stairway = ccToolsController.CreateHSREL5(Device.StairwayHSREL5, 60);
            var hspe8UpperFloor = ioBoardManager.GetOutputBoard(Device.UpperFloorAndOfficeHSPE8);
            var hspe16_FloorAndLowerBathroom = ccToolsController.CreateHSPE16OutputOnly(Device.LowerFloorAndLowerBathroomHSPE16, 17);

            var input1 = ioBoardManager.GetInputBoard(Device.Input1);
            var input2 = ioBoardManager.GetInputBoard(Device.Input2);
            var input4 = ioBoardManager.GetInputBoard(Device.Input4);

            const int SensorPin = 5; //4;

            var floor = home.AddRoom(Room.Floor)
                .WithMotionDetector(Floor.StairwayMotionDetector, input2.GetInput(1))
                .WithMotionDetector(Floor.StairsLowerMotionDetector, input4.GetInput(7))
                .WithMotionDetector(Floor.StairsUpperMotionDetector, input4.GetInput(6))
                .WithMotionDetector(Floor.LowerFloorMotionDetector, input1.GetInput(4))
                .WithTemperatureSensor(Floor.LowerFloorTemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Floor.LowerFloorHumiditySensor, dht22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(Floor.Lamp1, hspe16_FloorAndLowerBathroom.GetOutput(5).WithInvertedState())
                .WithLamp(Floor.Lamp2, hspe16_FloorAndLowerBathroom.GetOutput(6).WithInvertedState())
                .WithLamp(Floor.Lamp3, hspe16_FloorAndLowerBathroom.GetOutput(7).WithInvertedState())
                .WithLamp(Floor.StairwayLampCeiling, hsrel5Stairway.GetOutput(0))
                .WithLamp(Floor.StairwayLampWall, hsrel5Stairway.GetOutput(1))
                .WithRollerShutter(Floor.StairwayRollerShutter, hsrel5Stairway.GetOutput(4), hsrel5Stairway.GetOutput(3), RollerShutter.DefaultMaxMovingDuration, 20000)
                .WithButton(Floor.ButtonLowerFloorUpper, input1.GetInput(0))
                .WithButton(Floor.ButtonLowerFloorLower, input1.GetInput(5))
                .WithButton(Floor.ButtonLowerFloorAtBathroom, input1.GetInput(1))
                .WithButton(Floor.ButtonLowerFloorAtKitchen, input1.GetInput(3))
                .WithButton(Floor.ButtonStairsLowerUpper, input4.GetInput(5))
                .WithButton(Floor.ButtonStairsLowerLower, input1.GetInput(2))
                .WithButton(Floor.ButtonStairsUpper, input4.GetInput(4))
                .WithButton(Floor.ButtonStairway, input1.GetInput(6));

            floor.CombineActuators(Floor.CombinedStairwayLamp)
                .WithActuator(floor.Lamp(Floor.StairwayLampCeiling))
                .WithActuator(floor.Lamp(Floor.StairwayLampWall));

            floor.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(floor.MotionDetector(Floor.StairwayMotionDetector))
                .WithTrigger(floor.Button(Floor.ButtonStairway))
                .WithTarget(floor.BinaryStateOutput(Floor.CombinedStairwayLamp))
                .WithEnabledAtNight(home.WeatherStation)
                .WithOnDuration(TimeSpan.FromSeconds(30));

            floor.CombineActuators(Floor.CombinedLamps)
                .WithActuator(floor.Lamp(Floor.Lamp1))
                .WithActuator(floor.Lamp(Floor.Lamp2))
                .WithActuator(floor.Lamp(Floor.Lamp3));

            floor.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(floor.MotionDetector(Floor.LowerFloorMotionDetector))
                .WithTrigger(floor.Button(Floor.ButtonLowerFloorUpper))
                .WithTrigger(floor.Button(Floor.ButtonLowerFloorAtBathroom))
                .WithTrigger(floor.Button(Floor.ButtonLowerFloorAtKitchen))
                .WithTarget(floor.BinaryStateOutput(Floor.CombinedLamps))
                .WithEnabledAtNight(home.WeatherStation)
                .WithTurnOffIfButtonPressedWhileAlreadyOn()
                .WithOnDuration(TimeSpan.FromSeconds(20));

            SetupStairsCeilingLamps(floor, hspe8UpperFloor);
            SetupStairsLamps(floor, home.WeatherStation, hspe16_FloorAndLowerBathroom);
            
            floor.SetupAutomaticRollerShutters().WithRollerShutter(floor.RollerShutter(Floor.StairwayRollerShutter));
        }

        private void SetupStairsCeilingLamps(Actuators.Room floor, IBinaryOutputController hspe8UpperFloor)
        {
            floor.WithLamp(Floor.LampStairsCeiling1, hspe8UpperFloor.GetOutput(4).WithInvertedState())
                .WithLamp(Floor.LampStairsCeiling2, hspe8UpperFloor.GetOutput(5).WithInvertedState())
                .WithLamp(Floor.LampStairsCeiling3, hspe8UpperFloor.GetOutput(7).WithInvertedState())
                .WithLamp(Floor.LampStairsCeiling4, hspe8UpperFloor.GetOutput(6).WithInvertedState());

            var combinedLights = floor.CombineActuators(Floor.CombinedLampStairsCeiling)
                .WithActuator(floor.Lamp(Floor.LampStairsCeiling1))
                .WithActuator(floor.Lamp(Floor.LampStairsCeiling2))
                .WithActuator(floor.Lamp(Floor.LampStairsCeiling3))
                .WithActuator(floor.Lamp(Floor.LampStairsCeiling4));

            floor.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(floor.MotionDetector(Floor.StairsLowerMotionDetector), new AnimateParameter())
                .WithTrigger(floor.MotionDetector(Floor.StairsUpperMotionDetector))
                //.WithTrigger(floor.Button(Floor.ButtonStairsUpper))
                .WithTarget(floor.BinaryStateOutput(Floor.CombinedLampStairsCeiling))
                .WithOnDuration(TimeSpan.FromSeconds(10));

            floor.Button(Floor.ButtonStairsUpper).PressedShort += (s, e) =>
            {
                if (combinedLights.GetState() == BinaryActuatorState.On)
                {
                    combinedLights.TurnOff(new AnimateParameter().WithReversedOrder());
                }
                else
                {
                    combinedLights.TurnOn(new AnimateParameter());
                }
            };

            floor.Button(Floor.ButtonStairsLowerUpper).PressedShort += (s, e) =>
            {
                if (combinedLights.GetState() == BinaryActuatorState.On)
                {
                    combinedLights.TurnOff(new AnimateParameter());
                }
                else
                {
                    combinedLights.TurnOn(new AnimateParameter().WithReversedOrder());
                }
            };
        }

        private void SetupStairsLamps(Actuators.Room floor, IWeatherStation weatherStation, IBinaryOutputController hspe16_FloorAndLowerBathroom)
        {
            floor.WithLamp(Floor.LampStairs1, hspe16_FloorAndLowerBathroom.GetOutput(8).WithInvertedState())
                .WithLamp(Floor.LampStairs2, hspe16_FloorAndLowerBathroom.GetOutput(9).WithInvertedState())
                .WithLamp(Floor.LampStairs3, hspe16_FloorAndLowerBathroom.GetOutput(10).WithInvertedState())
                .WithLamp(Floor.LampStairs4, hspe16_FloorAndLowerBathroom.GetOutput(11).WithInvertedState())
                .WithLamp(Floor.LampStairs5, hspe16_FloorAndLowerBathroom.GetOutput(13).WithInvertedState())
                .WithLamp(Floor.LampStairs6, hspe16_FloorAndLowerBathroom.GetOutput(12).WithInvertedState());

            floor.SetupAlwaysOn()
                .WithActuator(floor.Lamp(Floor.LampStairs1))
                .WithActuator(floor.Lamp(Floor.LampStairs2))
                .WithActuator(floor.Lamp(Floor.LampStairs3))
                .WithActuator(floor.Lamp(Floor.LampStairs4))
                .WithActuator(floor.Lamp(Floor.LampStairs5))
                .WithActuator(floor.Lamp(Floor.LampStairs6))
                .WithOnlyAtNightRange(weatherStation)
                .WithOffBetweenRange(TimeSpan.FromHours(23), TimeSpan.FromHours(4));
        }
    }
}