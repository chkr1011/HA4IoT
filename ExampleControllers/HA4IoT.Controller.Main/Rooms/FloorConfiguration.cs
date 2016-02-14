using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Animations;
using HA4IoT.Actuators.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;

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

            LampStairsCeiling,
            LampStairs
        }

        public void Setup(Controller controller, CCToolsBoardController ccToolsController)
        {
            var hsrel5Stairway = ccToolsController.CreateHSREL5(Device.StairwayHSREL5, new I2CSlaveAddress(60));
            var hspe8UpperFloor = controller.Device<HSPE8OutputOnly>(Device.UpperFloorAndOfficeHSPE8);
            var hspe16FloorAndLowerBathroom = ccToolsController.CreateHSPE16OutputOnly(Device.LowerFloorAndLowerBathroomHSPE16, new I2CSlaveAddress(17));

            var input1 = controller.Device<HSPE16InputOnly>(Device.Input1);
            var input2 = controller.Device<HSPE16InputOnly>(Device.Input2);
            var input4 = controller.Device<HSPE16InputOnly>(Device.Input4);

            var i2cHardwareBridge = controller.Device<I2CHardwareBridge>();

            const int SensorPin = 5;

            var floor = controller.CreateRoom(Room.Floor)
                .WithMotionDetector(Floor.StairwayMotionDetector, input2.GetInput(1))
                .WithMotionDetector(Floor.StairsLowerMotionDetector, input4.GetInput(7))
                .WithMotionDetector(Floor.StairsUpperMotionDetector, input4.GetInput(6))
                .WithMotionDetector(Floor.LowerFloorMotionDetector, input1.GetInput(4))
                .WithTemperatureSensor(Floor.LowerFloorTemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Floor.LowerFloorHumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(Floor.Lamp1, hspe16FloorAndLowerBathroom.GetOutput(5).WithInvertedState())
                .WithLamp(Floor.Lamp2, hspe16FloorAndLowerBathroom.GetOutput(6).WithInvertedState())
                .WithLamp(Floor.Lamp3, hspe16FloorAndLowerBathroom.GetOutput(7).WithInvertedState())
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

            floor.SetupAutomaticTurnOnAndOffAutomation()
                .WithTrigger(floor.MotionDetector(Floor.StairwayMotionDetector))
                .WithTrigger(floor.Button(Floor.ButtonStairway).GetPressedShortlyTrigger())
                .WithTarget(floor.BinaryStateOutput(Floor.CombinedStairwayLamp))
                .WithEnabledAtNight(controller.WeatherStation)
                .WithOnDuration(TimeSpan.FromSeconds(30));

            floor.CombineActuators(Floor.CombinedLamps)
                .WithActuator(floor.Lamp(Floor.Lamp1))
                .WithActuator(floor.Lamp(Floor.Lamp2))
                .WithActuator(floor.Lamp(Floor.Lamp3));

            floor.SetupAutomaticTurnOnAndOffAutomation()
                .WithTrigger(floor.MotionDetector(Floor.LowerFloorMotionDetector))
                .WithTrigger(floor.Button(Floor.ButtonLowerFloorUpper).GetPressedShortlyTrigger())
                .WithTrigger(floor.Button(Floor.ButtonLowerFloorAtBathroom).GetPressedShortlyTrigger())
                .WithTrigger(floor.Button(Floor.ButtonLowerFloorAtKitchen).GetPressedShortlyTrigger())
                .WithTarget(floor.BinaryStateOutput(Floor.CombinedLamps))
                .WithEnabledAtNight(controller.WeatherStation)
                .WithTurnOffIfButtonPressedWhileAlreadyOn()
                .WithOnDuration(TimeSpan.FromSeconds(20));

            SetupStairsCeilingLamps(floor, hspe8UpperFloor);
            SetupStairsLamps(floor, controller.WeatherStation, hspe16FloorAndLowerBathroom);
            
            floor.SetupAutomaticRollerShutters().WithRollerShutters(floor.RollerShutter(Floor.StairwayRollerShutter));
        }

        private void SetupStairsCeilingLamps(IRoom floor, HSPE8OutputOnly hspe8UpperFloor)
        {
            var output = new LogicalBinaryOutput()
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO4])
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO5])
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO7])
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO6])
                .WithInvertedState();

            floor.WithLamp(Floor.LampStairsCeiling, output);

            floor.SetupAutomaticTurnOnAndOffAutomation()
                .WithTrigger(floor.MotionDetector(Floor.StairsLowerMotionDetector), new AnimateParameter())
                .WithTrigger(floor.MotionDetector(Floor.StairsUpperMotionDetector))
                //.WithTrigger(floor.Button(Floor.ButtonStairsUpper))
                .WithTarget(floor.BinaryStateOutput(Floor.LampStairsCeiling))
                .WithOnDuration(TimeSpan.FromSeconds(10));

            var lamp = floor.Lamp(Floor.LampStairsCeiling);

            floor.Button(Floor.ButtonStairsUpper).GetPressedShortlyTrigger().Triggered += (s, e) =>
            {
                if (lamp.GetState() == BinaryActuatorState.On)
                {
                    lamp.TurnOff(new AnimateParameter().WithReversedOrder());
                }
                else
                {
                    lamp.TurnOn(new AnimateParameter());
                }
            };

            floor.Button(Floor.ButtonStairsLowerUpper).GetPressedShortlyTrigger().Triggered += (s, e) =>
            {
                if (lamp.GetState() == BinaryActuatorState.On)
                {
                    lamp.TurnOff(new AnimateParameter());
                }
                else
                {
                    lamp.TurnOn(new AnimateParameter().WithReversedOrder());
                }
            };
        }

        private void SetupStairsLamps(IRoom floor, IWeatherStation weatherStation, HSPE16OutputOnly hspe16FloorAndLowerBathroom)
        {
            var output = new LogicalBinaryOutput()
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO8])
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO9])
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO10])
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO11])
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO13])
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO12])
                .WithInvertedState();

            floor.WithLamp(Floor.LampStairs, output);

            floor.SetupAutomaticConditionalOnAutomation()
                .WithActuator(floor.Lamp(Floor.LampStairs))
                .WithOnAtNightRange(weatherStation)
                .WithOffBetweenRange(TimeSpan.FromHours(23), TimeSpan.FromHours(4));
        }
    }
}