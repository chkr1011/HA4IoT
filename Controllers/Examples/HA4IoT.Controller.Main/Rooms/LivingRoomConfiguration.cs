using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
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

            WindowLeft,
            WindowRight,
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

            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0);
            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 12;

            var area = _areaService.RegisterArea(Room.LivingRoom);

            _sensorFactory.RegisterWindow(area, LivingRoom.WindowLeft,
                    w => w.WithLeftCasement(input0.GetInput(10), input0.GetInput(11)).WithRightCasement(input0.GetInput(9), input0.GetInput(8)));

            _sensorFactory.RegisterWindow(area, LivingRoom.WindowRight,
                    w => w.WithLeftCasement(input1.GetInput(14), input1.GetInput(15)).WithRightCasement(input1.GetInput(13), input1.GetInput(12)));

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

            area.GetLamp(LivingRoom.LampDiningTable)
                .ConnectToggleActionWith(area.GetButton(LivingRoom.ButtonUpper))
                .ConnectToggleActionWith(area.GetButton(LivingRoom.ButtonPassage));

            area.GetLamp(LivingRoom.LampCouch).
                ConnectToggleActionWith(area.GetButton(LivingRoom.ButtonMiddle));

            area.GetSocket(LivingRoom.SocketWallRightEdgeRight).
                ConnectToggleActionWith(area.GetButton(LivingRoom.ButtonLower));
        }
    }
}
