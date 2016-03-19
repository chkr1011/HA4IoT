using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Animations;
using HA4IoT.Automations;
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

            var i2cHardwareBridge = controller.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 5;

            var floor = controller.CreateArea(Room.Floor)
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
                .WithRollerShutter(Floor.StairwayRollerShutter, hsrel5Stairway.GetOutput(4), hsrel5Stairway.GetOutput(3))
                .WithButton(Floor.ButtonLowerFloorUpper, input1.GetInput(0))
                .WithButton(Floor.ButtonLowerFloorLower, input1.GetInput(5))
                .WithButton(Floor.ButtonLowerFloorAtBathroom, input1.GetInput(1))
                .WithButton(Floor.ButtonLowerFloorAtKitchen, input1.GetInput(3))
                .WithButton(Floor.ButtonStairsLowerUpper, input4.GetInput(5))
                .WithButton(Floor.ButtonStairsLowerLower, input1.GetInput(2))
                .WithButton(Floor.ButtonStairsUpper, input4.GetInput(4))
                .WithButton(Floor.ButtonStairway, input1.GetInput(6));

            floor.CombineActuators(Floor.CombinedStairwayLamp)
                .WithActuator(floor.GetLamp(Floor.StairwayLampCeiling))
                .WithActuator(floor.GetLamp(Floor.StairwayLampWall));

            floor.SetupTurnOnAndOffAutomation()
                .WithTrigger(floor.GetMotionDetector(Floor.StairwayMotionDetector))
                .WithTrigger(floor.GetButton(Floor.ButtonStairway).GetPressedShortlyTrigger())
                .WithTarget(floor.BinaryStateOutput(Floor.CombinedStairwayLamp))
                .WithEnabledAtNight(controller.GetDevice<IWeatherStation>())
                .WithOnDuration(TimeSpan.FromSeconds(30));

            floor.CombineActuators(Floor.CombinedLamps)
                .WithActuator(floor.GetLamp(Floor.Lamp1))
                .WithActuator(floor.GetLamp(Floor.Lamp2))
                .WithActuator(floor.GetLamp(Floor.Lamp3));

            floor.SetupTurnOnAndOffAutomation()
                .WithTrigger(floor.GetMotionDetector(Floor.LowerFloorMotionDetector))
                .WithTrigger(floor.GetButton(Floor.ButtonLowerFloorUpper).GetPressedShortlyTrigger())
                .WithTrigger(floor.GetButton(Floor.ButtonLowerFloorAtBathroom).GetPressedShortlyTrigger())
                .WithTrigger(floor.GetButton(Floor.ButtonLowerFloorAtKitchen).GetPressedShortlyTrigger())
                .WithTarget(floor.BinaryStateOutput(Floor.CombinedLamps))
                .WithEnabledAtNight(controller.GetDevice<IWeatherStation>())
                .WithTurnOffIfButtonPressedWhileAlreadyOn()
                .WithOnDuration(TimeSpan.FromSeconds(20));

            SetupStairsCeilingLamps(floor, hspe8UpperFloor);
            SetupStairsLamps(floor, controller.GetDevice<IWeatherStation>(), hspe16FloorAndLowerBathroom);
            
            floor.SetupRollerShutterAutomation().WithRollerShutters(floor.GetRollerShutter(Floor.StairwayRollerShutter));
        }

        private void SetupStairsCeilingLamps(IArea floor, HSPE8OutputOnly hspe8UpperFloor)
        {
            var output = new LogicalBinaryOutput()
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO4])
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO5])
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO7])
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO6])
                .WithInvertedState();

            floor.WithLamp(Floor.LampStairsCeiling, output);

            floor.SetupTurnOnAndOffAutomation()
                .WithTrigger(floor.GetMotionDetector(Floor.StairsLowerMotionDetector), new AnimateParameter())
                .WithTrigger(floor.GetMotionDetector(Floor.StairsUpperMotionDetector))
                //.WithTrigger(floor.Button(Floor.ButtonStairsUpper))
                .WithTarget(floor.BinaryStateOutput(Floor.LampStairsCeiling))
                .WithOnDuration(TimeSpan.FromSeconds(10));

            var lamp = floor.GetLamp(Floor.LampStairsCeiling);

            floor.GetButton(Floor.ButtonStairsUpper).GetPressedShortlyTrigger().Triggered += (s, e) =>
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

            floor.GetButton(Floor.ButtonStairsLowerUpper).GetPressedShortlyTrigger().Triggered += (s, e) =>
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

        private void SetupStairsLamps(IArea floor, IWeatherStation weatherStation, HSPE16OutputOnly hspe16FloorAndLowerBathroom)
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

            floor.SetupConditionalOnAutomation()
                .WithActuator(floor.GetLamp(Floor.LampStairs))
                .WithOnAtNightRange(weatherStation)
                .WithOffBetweenRange(TimeSpan.FromHours(23), TimeSpan.FromHours(4));
        }
    }
}