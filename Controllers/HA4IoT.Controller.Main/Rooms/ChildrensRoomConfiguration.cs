using System;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class ChildrensRoomRoomConfiguration
    {
        private readonly IAreaService _areaService;
        private readonly SynonymService _synonymService;
        private readonly IDeviceService _deviceService;
        private readonly CCToolsBoardService _ccToolsBoardService;

        private enum ChildrensRoom
        {
            TemperatureSensor,
            HumiditySensor,

            LightCeilingMiddle,

            RollerShutter,
            RollerShutterButtonUp,
            RollerShutterButtonDown,

            Button,

            SocketWindow,
            SocketWallLeft,
            SocketWallRight,

            Window
        }

        public ChildrensRoomRoomConfiguration(
            IAreaService areaService, 
            SynonymService synonymService,
            IDeviceService deviceService, 
            CCToolsBoardService ccToolsBoardService)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));

            _areaService = areaService;
            _synonymService = synonymService;
            _deviceService = deviceService;
            _ccToolsBoardService = ccToolsBoardService;
        }

        public void Setup()
        {
            var hsrel5 = _ccToolsBoardService.CreateHSREL5(InstalledDevice.ChildrensRoomHSREL5, new I2CSlaveAddress(63));
            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 7;

            var room = _areaService.CreateArea(Room.ChildrensRoom)
                .WithTemperatureSensor(ChildrensRoom.TemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(ChildrensRoom.HumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(ChildrensRoom.LightCeilingMiddle, hsrel5[HSREL5Pin.GPIO0])
                .WithRollerShutter(ChildrensRoom.RollerShutter, hsrel5[HSREL5Pin.Relay4], hsrel5[HSREL5Pin.Relay3])
                .WithSocket(ChildrensRoom.SocketWindow, hsrel5[HSREL5Pin.Relay0])
                .WithSocket(ChildrensRoom.SocketWallLeft, hsrel5[HSREL5Pin.Relay1])
                .WithSocket(ChildrensRoom.SocketWallRight, hsrel5[HSREL5Pin.Relay2])
                .WithButton(ChildrensRoom.Button, input0.GetInput(0))
                .WithWindow(ChildrensRoom.Window, w => w.WithCenterCasement(input0.GetInput(5), input0.GetInput(4)))
                .WithRollerShutterButtons(ChildrensRoom.RollerShutterButtonUp, input0.GetInput(1), ChildrensRoom.RollerShutterButtonDown, input0.GetInput(2));

            room.GetLamp(ChildrensRoom.LightCeilingMiddle).ConnectToggleActionWith(room.GetButton(ChildrensRoom.Button));

            room.SetupRollerShutterAutomation().WithRollerShutters(room.GetRollerShutter(ChildrensRoom.RollerShutter));
            room.GetRollerShutter(ChildrensRoom.RollerShutter)
                .ConnectWith(room.GetButton(ChildrensRoom.RollerShutterButtonUp), room.GetButton(ChildrensRoom.RollerShutterButtonDown));

            _synonymService.AddSynonymsForArea(Room.ChildrensRoom, "Kinderzimmer", "ChildrensRoom");
        }
    }
}
