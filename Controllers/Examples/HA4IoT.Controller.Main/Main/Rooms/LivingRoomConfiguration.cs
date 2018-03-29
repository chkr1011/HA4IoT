using System;
using HA4IoT.Actuators;
using HA4IoT.Areas;
using HA4IoT.Components;
using HA4IoT.Components.Adapters.MqttBased;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Hardware.Drivers.CCTools;
using HA4IoT.Hardware.Drivers.CCTools.Devices;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal class LivingRoomConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly IMessageBrokerService _messageBrokerService;
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;
        private readonly ILogService _logService;

        private enum LivingRoom
        {
            MotionDetector,
            TemperatureSensor,
            HumiditySensor,

            LampCouch,
            LampDiningTable,

            SocketWindowLeftUpper,
            SocketWindowLeftLower,
            SocketWindowMiddleLower,
            SocketWindowRightUpper,
            SocketWindowRightLower,

            SocketWallRightEdgeRight,
            SocketWallRightCenterLeft,
            SocketWallRightCenterMiddle,
            SocketWallRightCenterRight,

            SocketWallLeftEdgeLeft,
            SocketWallLeftCenterLeft,
            SocketWallLeftCenterMiddle,
            SocketWallLeftCenterRight,

            ButtonUpper,
            ButtonMiddle,
            ButtonLower,
            ButtonPassage,

            WindowLeftL,
            WindowLeftR,

            WindowRightL,
            WindowRightR,
        }

        public LivingRoomConfiguration(
            IDeviceRegistryService deviceService,
            IAreaRegistryService areaService,
            CCToolsDeviceService ccToolsBoardService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            IMessageBrokerService messageBrokerService,
            IDeviceMessageBrokerService deviceMessageBrokerService,
            ILogService logService)
        {
            _messageBrokerService = messageBrokerService ?? throw new ArgumentNullException(nameof(messageBrokerService));
            _deviceMessageBrokerService = deviceMessageBrokerService;
            _logService = logService;
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
        }

        public void Apply()
        {
            var hsrel8 = (HSREL8)_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSRel8, InstalledDevice.LivingRoomHSREL8.ToString(), 18);
            ////var hsrel5 = (HSREL5)_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSRel5, InstalledDevice.LivingRoomHSREL5.ToString(), 57);

            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0.ToString());
            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1.ToString());
            
            var area = _areaService.RegisterArea(Room.LivingRoom);

            _sensorFactory.RegisterWindow(area, LivingRoom.WindowLeftL, input0.GetInput(10), input0.GetInput(11));
            _sensorFactory.RegisterWindow(area, LivingRoom.WindowLeftR, input0.GetInput(9), input0.GetInput(8));
            _sensorFactory.RegisterWindow(area, LivingRoom.WindowRightL, input1.GetInput(14), input1.GetInput(15));
            _sensorFactory.RegisterWindow(area, LivingRoom.WindowRightR, input1.GetInput(13), input1.GetInput(12));

            _sensorFactory.RegisterTemperatureSensor(area, LivingRoom.TemperatureSensor,
                new MqttBasedNumericSensorAdapter("sensors-bridge/temperature/0", _deviceMessageBrokerService, _logService));

            _sensorFactory.RegisterHumiditySensor(area, LivingRoom.HumiditySensor,
                new MqttBasedNumericSensorAdapter("sensors-bridge/humidity/0", _deviceMessageBrokerService, _logService));

            _actuatorFactory.RegisterLamp(area, LivingRoom.LampCouch, hsrel8.GetOutput(8).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, LivingRoom.LampDiningTable, hsrel8.GetOutput(9).WithInvertedState());

            _actuatorFactory.RegisterSocket(area, LivingRoom.SocketWindowLeftLower, hsrel8.GetOutput(1));
            _actuatorFactory.RegisterSocket(area, LivingRoom.SocketWindowMiddleLower, hsrel8.GetOutput(2));
            _actuatorFactory.RegisterSocket(area, LivingRoom.SocketWindowRightLower, hsrel8.GetOutput(3));
            _actuatorFactory.RegisterSocket(area, LivingRoom.SocketWindowLeftUpper, hsrel8.GetOutput(5));
            _actuatorFactory.RegisterSocket(area, LivingRoom.SocketWindowRightUpper, hsrel8.GetOutput(7));
            _actuatorFactory.RegisterSocket(area, LivingRoom.SocketWallRightEdgeRight, hsrel8.GetOutput(4));
            _actuatorFactory.RegisterSocket(area, LivingRoom.SocketWallLeftEdgeLeft, hsrel8.GetOutput(0));
            
            _sensorFactory.RegisterButton(area, LivingRoom.ButtonUpper, input0.GetInput(15));
            _sensorFactory.RegisterButton(area, LivingRoom.ButtonMiddle, input0.GetInput(14));
            _sensorFactory.RegisterButton(area, LivingRoom.ButtonLower, input0.GetInput(13));
            _sensorFactory.RegisterButton(area, LivingRoom.ButtonPassage, input1.GetInput(10));

            area.GetButton(LivingRoom.ButtonUpper).CreatePressedShortTrigger(_messageBrokerService).Attach(
                () => area.GetComponent(LivingRoom.LampDiningTable).TryTogglePowerState());

            area.GetButton(LivingRoom.ButtonMiddle).CreatePressedShortTrigger(_messageBrokerService).Attach(() =>
                area.GetComponent(LivingRoom.LampCouch).TryTogglePowerState());
            
            area.GetButton(LivingRoom.ButtonLower).CreatePressedShortTrigger(_messageBrokerService).Attach(() => 
                area.GetComponent(LivingRoom.SocketWallRightEdgeRight).TryTogglePowerState());

            area.GetButton(LivingRoom.ButtonPassage).CreatePressedShortTrigger(_messageBrokerService).Attach(
                () => area.GetComponent(LivingRoom.LampDiningTable).TryTogglePowerState());
        }
    }
}
