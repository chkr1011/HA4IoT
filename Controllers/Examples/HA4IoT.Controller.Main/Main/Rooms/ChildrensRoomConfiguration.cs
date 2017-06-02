using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Areas;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Components.Adapters.PortBased;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Hardware.Drivers.CCTools;
using HA4IoT.Hardware.Drivers.CCTools.Devices;
using HA4IoT.Hardware.Drivers.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal class ChildrensRoomRoomConfiguration
    {
        private readonly IAreaRegistryService _areaService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly IMessageBrokerService _messageBroker;

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
            IAreaRegistryService areaService,
            IDeviceRegistryService deviceService,
            CCToolsDeviceService ccToolsBoardService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            IMessageBrokerService messageBroker)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
        }

        public void Apply()
        {
            var hsrel5 = (HSREL5)_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSRel5, InstalledDevice.ChildrensRoomHSREL5.ToString(), 63);
            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0.ToString());
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 7;

            var area = _areaService.RegisterArea(Room.ChildrensRoom);

            _sensorFactory.RegisterWindow(area, ChildrensRoom.Window, new PortBasedWindowAdapter(input0.GetInput(5), input0.GetInput(4)));

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
            _sensorFactory.RegisterButton(area, ChildrensRoom.RollerShutterButtonUp, input0.GetInput(1));
            _sensorFactory.RegisterButton(area, ChildrensRoom.RollerShutterButtonDown, input0.GetInput(2));

            area.GetButton(ChildrensRoom.Button).CreatePressedShortTrigger(_messageBroker).Attach(() => area.GetLamp(ChildrensRoom.LightCeilingMiddle).TryTogglePowerState());

            _automationFactory.RegisterRollerShutterAutomation(area, ChildrensRoom.RollerShutterAutomation)
                .WithRollerShutters(area.GetRollerShutter(ChildrensRoom.RollerShutter));

            area.GetRollerShutter(ChildrensRoom.RollerShutter)
                .ConnectWith(area.GetButton(ChildrensRoom.RollerShutterButtonUp), area.GetButton(ChildrensRoom.RollerShutterButtonDown), _messageBroker);
        }
    }
}
