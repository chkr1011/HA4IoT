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
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class ChildrensRoomRoomConfiguration
    {
        private readonly IAreaService _areaService;
        private readonly IDeviceService _deviceService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;

        private enum ChildrensRoom
        {
            TemperatureSensor,
            HumiditySensor,

            LightCeilingMiddle,

            RollerShutter,
            RollerShutterButtonUp,
            RollerShutterButtonDown,
            RollerShutterAutomation,

            Button,

            SocketWindow,
            SocketWallLeft,
            SocketWallRight,

            Window
        }

        public ChildrensRoomRoomConfiguration(
            IAreaService areaService,
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
            var hsrel5 = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.ChildrensRoomHSREL5, new I2CSlaveAddress(63));
            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 7;

            var area = _areaService.CreateArea(Room.ChildrensRoom);

            _sensorFactory.RegisterWindow(area, ChildrensRoom.Window, w => w.WithCenterCasement(input0.GetInput(5), input0.GetInput(4)));

            _sensorFactory.RegisterTemperatureSensor(area, ChildrensRoom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, ChildrensRoom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _actuatorFactory.RegisterSocket(area, ChildrensRoom.SocketWindow, hsrel5[HSREL5Pin.Relay0]);
            _actuatorFactory.RegisterSocket(area, ChildrensRoom.SocketWallLeft, hsrel5[HSREL5Pin.Relay1]);
            _actuatorFactory.RegisterSocket(area, ChildrensRoom.SocketWallRight, hsrel5[HSREL5Pin.Relay2]);

            _actuatorFactory.RegisterLamp(area, ChildrensRoom.LightCeilingMiddle, hsrel5[HSREL5Pin.GPIO0]);

            _sensorFactory.RegisterButton(area, ChildrensRoom.Button, input0.GetInput(0));

            _actuatorFactory.RegisterRollerShutter(area, ChildrensRoom.RollerShutter, hsrel5[HSREL5Pin.Relay4], hsrel5[HSREL5Pin.Relay3]);
            _sensorFactory.RegisterRollerShutterButtons(area, ChildrensRoom.RollerShutterButtonUp, input0.GetInput(1), ChildrensRoom.RollerShutterButtonDown, input0.GetInput(2));

            area.GetLamp(ChildrensRoom.LightCeilingMiddle).ConnectToggleActionWith(area.GetButton(ChildrensRoom.Button));

            _automationFactory.RegisterRollerShutterAutomation(area, ChildrensRoom.RollerShutterAutomation)
                .WithRollerShutters(area.GetRollerShutter(ChildrensRoom.RollerShutter));

            area.GetRollerShutter(ChildrensRoom.RollerShutter)
                .ConnectWith(area.GetButton(ChildrensRoom.RollerShutterButtonUp), area.GetButton(ChildrensRoom.RollerShutterButtonDown));
        }
    }
}
