using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Animations;
using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class FloorConfiguration
    {
        private readonly IAreaService _areaService;
        private readonly IDaylightService _daylightService;
        private readonly IDeviceService _deviceService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly SynonymService _synonymService;

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

        public FloorConfiguration(
            IAreaService areaService,
            IDaylightService daylightService,
            IDeviceService deviceService,
            CCToolsBoardService ccToolsBoardService,
            SynonymService synonymService)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));

            _areaService = areaService;
            _daylightService = daylightService;
            _deviceService = deviceService;
            _ccToolsBoardService = ccToolsBoardService;
            _synonymService = synonymService;
        }

        public void Setup()
        {
            var hsrel5Stairway = _ccToolsBoardService.CreateHSREL5(InstalledDevice.StairwayHSREL5, new I2CSlaveAddress(60));
            var hspe8UpperFloor = _deviceService.GetDevice<HSPE8OutputOnly>(InstalledDevice.UpperFloorAndOfficeHSPE8);
            var hspe16FloorAndLowerBathroom = _ccToolsBoardService.CreateHSPE16OutputOnly(InstalledDevice.LowerFloorAndLowerBathroomHSPE16, new I2CSlaveAddress(17));

            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1);
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input2);
            var input4 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input4);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 5;

            var room = _areaService.CreateArea(Room.Floor)
                .WithMotionDetector(Floor.StairwayMotionDetector, input2.GetInput(1))
                .WithMotionDetector(Floor.StairsLowerMotionDetector, input4.GetInput(7))
                .WithMotionDetector(Floor.StairsUpperMotionDetector, input4.GetInput(6))
                .WithMotionDetector(Floor.LowerFloorMotionDetector, input1.GetInput(4))
                .WithTemperatureSensor(Floor.LowerFloorTemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Floor.LowerFloorHumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
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

            room.CombineActuators(Floor.CombinedStairwayLamp)
                .WithActuator(room.GetLamp(Floor.StairwayLampCeiling))
                .WithActuator(room.GetLamp(Floor.StairwayLampWall));

            SetupStairwayLamps(room);

            room.CombineActuators(Floor.CombinedLamps)
                .WithActuator(room.GetLamp(Floor.Lamp1))
                .WithActuator(room.GetLamp(Floor.Lamp2))
                .WithActuator(room.GetLamp(Floor.Lamp3));

            room.SetupTurnOnAndOffAutomation()
                .WithTrigger(room.GetMotionDetector(Floor.LowerFloorMotionDetector))
                .WithTrigger(room.GetButton(Floor.ButtonLowerFloorUpper).GetPressedShortlyTrigger())
                .WithTrigger(room.GetButton(Floor.ButtonLowerFloorAtBathroom).GetPressedShortlyTrigger())
                .WithTrigger(room.GetButton(Floor.ButtonLowerFloorAtKitchen).GetPressedShortlyTrigger())
                .WithTarget(room.GetActuator(Floor.CombinedLamps))
                .WithEnabledAtNight(_daylightService)
                .WithTurnOffIfButtonPressedWhileAlreadyOn()
                .WithOnDuration(TimeSpan.FromSeconds(20));

            SetupStairsCeilingLamps(room, hspe8UpperFloor);
            SetupStairsLamps(room, hspe16FloorAndLowerBathroom);
            
            room.SetupRollerShutterAutomation().WithRollerShutters(room.GetRollerShutter(Floor.StairwayRollerShutter));

            _synonymService.AddSynonymsForArea(Room.Floor, "Flur", "Floor");
        }

        private void SetupStairwayLamps(IArea room)
        {
            room.SetupTurnOnAndOffAutomation()
                .WithTrigger(room.GetMotionDetector(Floor.StairwayMotionDetector))
                .WithTrigger(room.GetButton(Floor.ButtonStairway).GetPressedShortlyTrigger())
                .WithTarget(room.GetActuator(Floor.CombinedStairwayLamp))
                .WithEnabledAtNight(_daylightService)
                .WithOnDuration(TimeSpan.FromSeconds(30));
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
                .WithTrigger(floor.GetMotionDetector(Floor.StairsLowerMotionDetector))
                .WithTrigger(floor.GetMotionDetector(Floor.StairsUpperMotionDetector))
                //.WithTrigger(floor.GetButton(Floor.ButtonStairsUpper).GetPressedShortlyTrigger())
                .WithTarget(floor.GetStateMachine(Floor.LampStairsCeiling))
                .WithOnDuration(TimeSpan.FromSeconds(10));

            var lamp = floor.GetLamp(Floor.LampStairsCeiling);

            floor.GetButton(Floor.ButtonStairsUpper).GetPressedShortlyTrigger().Triggered += (s, e) =>
            {
                if (lamp.GetState().Equals(BinaryStateId.On))
                {
                    lamp.SetState(BinaryStateId.Off, new AnimateParameter().WithReversedOrder());
                }
                else
                {
                    lamp.SetState(BinaryStateId.On, new AnimateParameter());
                }
            };

            floor.GetButton(Floor.ButtonStairsLowerUpper).GetPressedShortlyTrigger().Triggered += (s, e) =>
            {
                if (lamp.GetState().Equals(BinaryStateId.On))
                {
                    lamp.SetState(BinaryStateId.Off, new AnimateParameter());
                }
                else
                {
                    lamp.SetState(BinaryStateId.On, new AnimateParameter().WithReversedOrder());
                }
            };
        }

        private void SetupStairsLamps(IArea floor, HSPE16OutputOnly hspe16FloorAndLowerBathroom)
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
                .WithOnAtNightRange()
                .WithOffBetweenRange(TimeSpan.FromHours(23), TimeSpan.FromHours(4));
        }
    }
}