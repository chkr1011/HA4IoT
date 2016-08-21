using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class ReadingRoomConfiguration
    {
        private readonly IAreaService _areaService;
        private readonly SynonymService _synonymService;
        private readonly IDeviceService _deviceService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;

        private enum ReadingRoom
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

        public ReadingRoomConfiguration(
            IAreaService areaService,
            SynonymService synonymService,
            IDeviceService deviceService,
            CCToolsBoardService ccToolsBoardService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _areaService = areaService;
            _synonymService = synonymService;
            _deviceService = deviceService;
            _ccToolsBoardService = ccToolsBoardService;
            _automationFactory = automationFactory;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hsrel5 = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.ReadingRoomHSREL5, new I2CSlaveAddress(62));
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input2);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 9;

            var room = _areaService.CreateArea(Room.ReadingRoom);

            _sensorFactory.RegisterWindow(room, ReadingRoom.Window, w => w.WithCenterCasement(input2.GetInput(8))); // Tilt = input2.GetInput(9) -- currently broken!

            _sensorFactory.RegisterTemperatureSensor(room, ReadingRoom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(room, ReadingRoom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _actuatorFactory.RegisterLamp(room, ReadingRoom.LightCeilingMiddle, hsrel5[HSREL5Pin.GPIO0]);

            _actuatorFactory.RegisterRollerShutter(room, ReadingRoom.RollerShutter, hsrel5[HSREL5Pin.Relay4], hsrel5[HSREL5Pin.Relay3]);
            _sensorFactory.RegisterRollerShutterButtons(room, ReadingRoom.RollerShutterButtonUp, input2.GetInput(12),
                ReadingRoom.RollerShutterButtonDown, input2.GetInput(11));

            _actuatorFactory.RegisterSocket(room, ReadingRoom.SocketWindow, hsrel5[HSREL5Pin.Relay0]);
            _actuatorFactory.RegisterSocket(room, ReadingRoom.SocketWallLeft, hsrel5[HSREL5Pin.Relay1]);
            _actuatorFactory.RegisterSocket(room, ReadingRoom.SocketWallRight, hsrel5[HSREL5Pin.Relay2]);

            _sensorFactory.RegisterButton(room, ReadingRoom.Button, input2.GetInput(13));

            room.GetLamp(ReadingRoom.LightCeilingMiddle).ConnectToggleActionWith(room.GetButton(ReadingRoom.Button));

            _automationFactory.RegisterRollerShutterAutomation(room)
                .WithRollerShutters(room.GetRollerShutter(ReadingRoom.RollerShutter));

            room.GetRollerShutter(ReadingRoom.RollerShutter)
                .ConnectWith(room.GetButton(ReadingRoom.RollerShutterButtonUp), room.GetButton(ReadingRoom.RollerShutterButtonDown));

            _synonymService.AddSynonymsForArea(Room.ReadingRoom, "Lesezimmer", "Gästezimmer", "ReadingRoom");
        }
    }
}
