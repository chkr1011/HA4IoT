using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Animations;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class FloorConfiguration
    {
        private readonly IAreaRespositoryService _areaService;
        private readonly IDeviceService _deviceService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;

        private enum Floor
        {
            StairwayMotionDetector,

            StairwayLampWall,
            StairwayLampCeiling,
            CombinedStairwayLamp,
            CombinedStairwayLampAutomation,

            StairwayRollerShutter,
            StairwayRollerShutterAutomation,

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
            CombinedLampsAutomation,

            LampStairsCeiling,
            LampStairsCeilingAutomation,
            LampStairs,
            LampStairsAutomation
        }

        public FloorConfiguration(
            IAreaRespositoryService areaService,
            IDeviceService deviceService,
            CCToolsBoardService ccToolsBoardService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _areaService = areaService;
            _deviceService = deviceService;
            _ccToolsBoardService = ccToolsBoardService;
            _automationFactory = automationFactory;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hsrel5Stairway = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.StairwayHSREL5, new I2CSlaveAddress(60));
            var hspe8UpperFloor = _deviceService.GetDevice<HSPE8OutputOnly>(InstalledDevice.UpperFloorAndOfficeHSPE8);
            var hspe16FloorAndLowerBathroom = _ccToolsBoardService.RegisterHSPE16OutputOnly(InstalledDevice.LowerFloorAndLowerBathroomHSPE16, new I2CSlaveAddress(17));

            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1);
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input2);
            var input4 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input4);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 5;

            var area = _areaService.CreateArea(Room.Floor);

            _sensorFactory.RegisterTemperatureSensor(area, Floor.LowerFloorTemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, Floor.LowerFloorHumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(area, Floor.StairwayMotionDetector, input2.GetInput(1));
            _sensorFactory.RegisterMotionDetector(area, Floor.StairsLowerMotionDetector, input4.GetInput(7));
            _sensorFactory.RegisterMotionDetector(area, Floor.StairsUpperMotionDetector, input4.GetInput(6));
            _sensorFactory.RegisterMotionDetector(area, Floor.LowerFloorMotionDetector, input1.GetInput(4));

            _actuatorFactory.RegisterRollerShutter(area, Floor.StairwayRollerShutter, hsrel5Stairway.GetOutput(4), hsrel5Stairway.GetOutput(3));

            _sensorFactory.RegisterButton(area, Floor.ButtonLowerFloorUpper, input1.GetInput(0));
            _sensorFactory.RegisterButton(area, Floor.ButtonLowerFloorLower, input1.GetInput(5));
            _sensorFactory.RegisterButton(area, Floor.ButtonLowerFloorAtBathroom, input1.GetInput(1));
            _sensorFactory.RegisterButton(area, Floor.ButtonLowerFloorAtKitchen, input1.GetInput(3));
            _sensorFactory.RegisterButton(area, Floor.ButtonStairsLowerUpper, input4.GetInput(5));
            _sensorFactory.RegisterButton(area, Floor.ButtonStairsLowerLower, input1.GetInput(2));
            _sensorFactory.RegisterButton(area, Floor.ButtonStairsUpper, input4.GetInput(4));
            _sensorFactory.RegisterButton(area, Floor.ButtonStairway, input1.GetInput(6));

            _actuatorFactory.RegisterLamp(area, Floor.Lamp1, hspe16FloorAndLowerBathroom.GetOutput(5).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Floor.Lamp2, hspe16FloorAndLowerBathroom.GetOutput(6).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Floor.Lamp3, hspe16FloorAndLowerBathroom.GetOutput(7).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Floor.StairwayLampCeiling, hsrel5Stairway.GetOutput(0));
            _actuatorFactory.RegisterLamp(area, Floor.StairwayLampWall, hsrel5Stairway.GetOutput(1));

            _actuatorFactory.RegisterLogicalActuator(area, Floor.CombinedStairwayLamp)
                .WithActuator(area.GetLamp(Floor.StairwayLampCeiling))
                .WithActuator(area.GetLamp(Floor.StairwayLampWall));

            SetupStairwayLamps(area);

            _actuatorFactory.RegisterLogicalActuator(area, Floor.CombinedLamps)
                .WithActuator(area.GetLamp(Floor.Lamp1))
                .WithActuator(area.GetLamp(Floor.Lamp2))
                .WithActuator(area.GetLamp(Floor.Lamp3));

            _automationFactory.RegisterTurnOnAndOffAutomation(area, Floor.CombinedLampsAutomation)
                .WithTrigger(area.GetMotionDetector(Floor.LowerFloorMotionDetector))
                .WithFlipTrigger(area.GetButton(Floor.ButtonLowerFloorUpper).GetPressedShortlyTrigger())
                .WithFlipTrigger(area.GetButton(Floor.ButtonLowerFloorAtBathroom).GetPressedShortlyTrigger())
                .WithFlipTrigger(area.GetButton(Floor.ButtonLowerFloorAtKitchen).GetPressedShortlyTrigger())
                .WithTarget(area.GetActuator(Floor.CombinedLamps))
                .WithEnabledAtNight()
                .WithTurnOffIfButtonPressedWhileAlreadyOn();

            SetupStairsCeilingLamps(area, hspe8UpperFloor);
            SetupStairsLamps(area, hspe16FloorAndLowerBathroom);

            _automationFactory.RegisterRollerShutterAutomation(area, Floor.StairwayRollerShutterAutomation)
                .WithRollerShutters(area.GetRollerShutter(Floor.StairwayRollerShutter));
        }

        private void SetupStairwayLamps(IArea room)
        {
            _automationFactory.RegisterTurnOnAndOffAutomation(room, Floor.CombinedStairwayLampAutomation)
                .WithTrigger(room.GetMotionDetector(Floor.StairwayMotionDetector))
                .WithFlipTrigger(room.GetButton(Floor.ButtonStairway).GetPressedShortlyTrigger())
                .WithTarget(room.GetActuator(Floor.CombinedStairwayLamp))
                .WithEnabledAtNight();
        }

        private void SetupStairsCeilingLamps(IArea room, HSPE8OutputOnly hspe8UpperFloor)
        {
            var output = new LogicalBinaryOutput()
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO4])
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO5])
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO7])
                .WithOutput(hspe8UpperFloor[HSPE8Pin.GPIO6])
                .WithInvertedState();

            _actuatorFactory.RegisterLamp(room, Floor.LampStairsCeiling, output);

            _automationFactory.RegisterTurnOnAndOffAutomation(room, Floor.LampStairsCeilingAutomation)
                .WithTrigger(room.GetMotionDetector(Floor.StairsLowerMotionDetector))
                .WithTrigger(room.GetMotionDetector(Floor.StairsUpperMotionDetector))
                //.WithTrigger(floor.GetButton(Floor.ButtonStairsUpper).GetPressedShortlyTrigger())
                .WithTarget(room.GetStateMachine(Floor.LampStairsCeiling));

            var lamp = room.GetLamp(Floor.LampStairsCeiling);

            room.GetButton(Floor.ButtonStairsUpper).GetPressedShortlyTrigger().Triggered += (s, e) =>
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

            room.GetButton(Floor.ButtonStairsLowerUpper).GetPressedShortlyTrigger().Triggered += (s, e) =>
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

        private void SetupStairsLamps(IArea room, HSPE16OutputOnly hspe16FloorAndLowerBathroom)
        {
            var output = new LogicalBinaryOutput()
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO8])
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO9])
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO10])
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO11])
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO13])
                .WithOutput(hspe16FloorAndLowerBathroom[HSPE16Pin.GPIO12])
                .WithInvertedState();

            _actuatorFactory.RegisterLamp(room, Floor.LampStairs, output);

            _automationFactory.RegisterConditionalOnAutomation(room, Floor.LampStairsAutomation)
                .WithActuator(room.GetLamp(Floor.LampStairs))
                .WithOnAtNightRange()
                .WithOffBetweenRange(TimeSpan.FromHours(23), TimeSpan.FromHours(4));
        }
    }
}