using System;
using HA4IoT.Actuators;
using HA4IoT.Components;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Services.Areas;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal class LivingRoomConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly IMessageBrokerService _messageBroker;

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
            IMessageBrokerService messageBroker)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
        }

        public void Apply()
        {
            var hsrel8 = (HSREL8)_ccToolsBoardService.RegisterDevice(CCToolsDevice.HSRel8, InstalledDevice.LivingRoomHSREL8.ToString(), 18);
            var hsrel5 = (HSREL5)_ccToolsBoardService.RegisterDevice(CCToolsDevice.HSRel5, InstalledDevice.LivingRoomHSREL5.ToString(), 57);

            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0.ToString());
            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1.ToString());
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 12;

            var area = _areaService.RegisterArea(Room.LivingRoom);

            _sensorFactory.RegisterWindow(area, LivingRoom.WindowLeftL, input0.GetInput(10), input0.GetInput(11));
            _sensorFactory.RegisterWindow(area, LivingRoom.WindowLeftR, input0.GetInput(9), input0.GetInput(8));
            _sensorFactory.RegisterWindow(area, LivingRoom.WindowRightL, input1.GetInput(14), input1.GetInput(15));
            _sensorFactory.RegisterWindow(area, LivingRoom.WindowRightR, input1.GetInput(13), input1.GetInput(12));

            _sensorFactory.RegisterTemperatureSensor(area, LivingRoom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, LivingRoom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

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

            area.GetButton(LivingRoom.ButtonUpper).CreatePressedShortTrigger(_messageBroker).Attach(
                () => area.GetComponent(LivingRoom.LampDiningTable).TryTogglePowerState());

            area.GetButton(LivingRoom.ButtonMiddle).CreatePressedShortTrigger(_messageBroker).Attach(() =>
                area.GetComponent(LivingRoom.LampCouch).TryTogglePowerState());
            
            area.GetButton(LivingRoom.ButtonLower).CreatePressedShortTrigger(_messageBroker).Attach(() => 
                area.GetComponent(LivingRoom.SocketWallRightEdgeRight).TryTogglePowerState());

            area.GetButton(LivingRoom.ButtonPassage).CreatePressedShortTrigger(_messageBroker).Attach(
                () => area.GetComponent(LivingRoom.LampDiningTable).TryTogglePowerState());
        }
    }
}
