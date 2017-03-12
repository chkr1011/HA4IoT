using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Adapters;
using HA4IoT.Components;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
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
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;

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
            CCToolsBoardService ccToolsBoardService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _deviceService = deviceService;
            _areaService = areaService;
            _ccToolsBoardService = ccToolsBoardService;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hsrel8 = _ccToolsBoardService.RegisterHSREL8(InstalledDevice.LivingRoomHSREL8, new I2CSlaveAddress(18));
            var hsrel5 = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.LivingRoomHSREL5, new I2CSlaveAddress(57));

            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0.ToString());
            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1.ToString());
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 12;

            var area = _areaService.RegisterArea(Room.LivingRoom);

            _sensorFactory.RegisterWindow(area, LivingRoom.WindowLeftL,
                    new PortBasedWindowAdapter(input0.GetInput(10), input0.GetInput(11)));

            _sensorFactory.RegisterWindow(area, LivingRoom.WindowLeftR,
                new PortBasedWindowAdapter(input0.GetInput(9), input0.GetInput(8)));

            _sensorFactory.RegisterWindow(area, LivingRoom.WindowRightL,
                    new PortBasedWindowAdapter(input1.GetInput(14), input1.GetInput(15)));

            _sensorFactory.RegisterWindow(area, LivingRoom.WindowRightR,
                new PortBasedWindowAdapter(input1.GetInput(13), input1.GetInput(12)));

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

            area.GetButton(LivingRoom.ButtonUpper).PressedShortlyTrigger.Attach(
                () => area.GetLamp(LivingRoom.LampDiningTable).TryTogglePowerState());

            area.GetButton(LivingRoom.ButtonPassage).PressedShortlyTrigger.Attach(
                () => area.GetLamp(LivingRoom.LampDiningTable).TryTogglePowerState());

            area.GetButton(LivingRoom.ButtonLower).PressedShortlyTrigger.Attach(() => 
                area.GetSocket(LivingRoom.SocketWallRightEdgeRight).TryTogglePowerState());
        }
    }
}
