using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal class FloorConfiguration
    {
        private readonly IAreaRegistryService _areaService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
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
            IAreaRegistryService areaService,
            IDeviceRegistryService deviceService,
            CCToolsDeviceService ccToolsBoardService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
        }

        public void Apply()
        {
            var hsrel5Stairway = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.StairwayHSREL5.ToString(), new I2CSlaveAddress(60));
            var hspe8UpperFloor = _deviceService.GetDevice<HSPE8OutputOnly>(InstalledDevice.UpperFloorAndOfficeHSPE8.ToString());
            var hspe16FloorAndLowerBathroom = _ccToolsBoardService.RegisterHSPE16OutputOnly(InstalledDevice.LowerFloorAndLowerBathroomHSPE16.ToString(), new I2CSlaveAddress(17));

            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1.ToString());
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input2.ToString());
            var input4 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input4.ToString());
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 3;

            var area = _areaService.RegisterArea(Room.Floor);

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

            _actuatorFactory.RegisterLogicalComponent(area, Floor.CombinedStairwayLamp)
                .WithComponent(area.GetLamp(Floor.StairwayLampCeiling))
                .WithComponent(area.GetLamp(Floor.StairwayLampWall));

            SetupStairwayLamps(area);

            _actuatorFactory.RegisterLogicalComponent(area, Floor.CombinedLamps)
                .WithComponent(area.GetLamp(Floor.Lamp1))
                .WithComponent(area.GetLamp(Floor.Lamp2))
                .WithComponent(area.GetLamp(Floor.Lamp3));

            _automationFactory.RegisterTurnOnAndOffAutomation(area, Floor.CombinedLampsAutomation)
                .WithTrigger(area.GetMotionDetector(Floor.LowerFloorMotionDetector))
                .WithTrigger(area.GetButton(Floor.ButtonLowerFloorUpper).PressedShortTrigger)
                .WithTrigger(area.GetButton(Floor.ButtonLowerFloorAtBathroom).PressedShortTrigger)
                .WithTrigger(area.GetButton(Floor.ButtonLowerFloorAtKitchen).PressedShortTrigger)
                .WithTarget(area.GetComponent(Floor.CombinedLamps))
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
                .WithTrigger(room.GetButton(Floor.ButtonStairway).PressedShortTrigger)
                .WithTarget(room.GetComponent(Floor.CombinedStairwayLamp))
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
                .WithTarget(room.GetLamp(Floor.LampStairsCeiling));

            var lamp = room.GetLamp(Floor.LampStairsCeiling);

            room.GetButton(Floor.ButtonStairsUpper).PressedShortTrigger.Attach(() => lamp.TryTogglePowerState());
            room.GetButton(Floor.ButtonStairsLowerUpper).PressedShortTrigger.Attach(() => lamp.TryTogglePowerState());
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
                .WithComponent(room.GetLamp(Floor.LampStairs))
                .WithOnAtNightRange()
                .WithOffBetweenRange(TimeSpan.FromHours(23), TimeSpan.FromHours(4));
        }
    }
}