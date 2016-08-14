using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class KitchenConfiguration
    {
        private readonly IAreaService _areaService;
        private readonly IDaylightService _daylightService;
        private readonly IDeviceService _deviceService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly SynonymService _synonymService;

        public enum Kitchen
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingMiddle,
            LightCeilingWall,
            LightCeilingWindow,
            LightCeilingDoor,
            LightCeilingPassageOuter,
            LightCeilingPassageInner,
            CombinedAutomaticLights,

            RollerShutter,
            RollerShutterButtonUp,
            RollerShutterButtonDown,

            ButtonPassage,
            ButtonKitchenette,

            SocketWall,
            SocketKitchenette,

            Window
        }

        public KitchenConfiguration(
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
            var hsrel5 = _ccToolsBoardService.CreateHSREL5(InstalledDevice.KitchenHSREL5, new I2CSlaveAddress(58));
            var hspe8 = _ccToolsBoardService.CreateHSPE8OutputOnly(InstalledDevice.KitchenHSPE8, new I2CSlaveAddress(39));

            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0);
            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1);
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input2);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 11;

            var room = _areaService.CreateArea(Room.Kitchen)
                .WithTemperatureSensor(Kitchen.TemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Kitchen.HumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithMotionDetector(Kitchen.MotionDetector, input1.GetInput(8))
                .WithLamp(Kitchen.LightCeilingMiddle, hsrel5.GetOutput(5).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingWindow, hsrel5.GetOutput(6).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingWall, hsrel5.GetOutput(7).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingDoor, hspe8.GetOutput(0).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingPassageInner, hspe8.GetOutput(1).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingPassageOuter, hspe8.GetOutput(2).WithInvertedState())
                .WithSocket(Kitchen.SocketWall, hsrel5.GetOutput(2))
                .WithRollerShutter(Kitchen.RollerShutter, hsrel5.GetOutput(4), hsrel5.GetOutput(3))
                .WithButton(Kitchen.ButtonKitchenette, input1.GetInput(11))
                .WithButton(Kitchen.ButtonPassage, input1.GetInput(9))
                .WithRollerShutterButtons(Kitchen.RollerShutterButtonUp, input2.GetInput(15), Kitchen.RollerShutterButtonDown, input2.GetInput(14))
                .WithWindow(Kitchen.Window, w => w.WithCenterCasement(input0.GetInput(6), input0.GetInput(7)));

            room.GetLamp(Kitchen.LightCeilingMiddle).ConnectToggleActionWith(room.GetButton(Kitchen.ButtonKitchenette));
            room.GetLamp(Kitchen.LightCeilingMiddle).ConnectToggleActionWith(room.GetButton(Kitchen.ButtonPassage));

            room.SetupRollerShutterAutomation().WithRollerShutters(room.GetRollerShutter(Kitchen.RollerShutter));

            room.GetRollerShutter(Kitchen.RollerShutter).ConnectWith(
                room.GetButton(Kitchen.RollerShutterButtonUp), room.GetButton(Kitchen.RollerShutterButtonDown));

            room.CombineActuators(Kitchen.CombinedAutomaticLights)
                .WithActuator(room.GetLamp(Kitchen.LightCeilingWall))
                .WithActuator(room.GetLamp(Kitchen.LightCeilingDoor))
                .WithActuator(room.GetLamp(Kitchen.LightCeilingWindow));

            room.SetupTurnOnAndOffAutomation()
                .WithTrigger(room.GetMotionDetector(Kitchen.MotionDetector))
                .WithTarget(room.GetActuator(Kitchen.CombinedAutomaticLights))
                .WithEnabledAtNight(_daylightService);

            _synonymService.AddSynonymsForArea(Room.Kitchen, "Küche", "Kitchen");
        }
    }
}
