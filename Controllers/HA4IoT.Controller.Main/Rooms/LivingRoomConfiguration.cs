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
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly SynonymService _synonymService;
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
            IDeviceService deviceService,
            IAreaService areaService,
            CCToolsBoardService ccToolsBoardService,
            SynonymService synonymService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _deviceService = deviceService;
            _areaService = areaService;
            _ccToolsBoardService = ccToolsBoardService;
            _synonymService = synonymService;
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

            var room = _areaService.CreateArea(Room.LivingRoom);

            _sensorFactory.RegisterWindow(room, LivingRoom.WindowLeft,
                    w => w.WithLeftCasement(input0.GetInput(10), input0.GetInput(11)).WithRightCasement(input0.GetInput(9), input0.GetInput(8)));

            _sensorFactory.RegisterWindow(room, LivingRoom.WindowRight,
                    w => w.WithLeftCasement(input1.GetInput(14), input1.GetInput(15)).WithRightCasement(input1.GetInput(13), input1.GetInput(12)));

            _sensorFactory.RegisterTemperatureSensor(room, LivingRoom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(room, LivingRoom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _actuatorFactory.RegisterLamp(room, LivingRoom.LampCouch, hsrel8.GetOutput(8).WithInvertedState());
            _actuatorFactory.RegisterLamp(room, LivingRoom.LampDiningTable, hsrel8.GetOutput(9).WithInvertedState());

            _actuatorFactory.RegisterSocket(room, LivingRoom.SocketWindowLeftLower, hsrel8.GetOutput(1));
            _actuatorFactory.RegisterSocket(room, LivingRoom.SocketWindowMiddleLower, hsrel8.GetOutput(2));
            _actuatorFactory.RegisterSocket(room, LivingRoom.SocketWindowRightLower, hsrel8.GetOutput(3));
            _actuatorFactory.RegisterSocket(room, LivingRoom.SocketWindowLeftUpper, hsrel8.GetOutput(5));
            _actuatorFactory.RegisterSocket(room, LivingRoom.SocketWindowRightUpper, hsrel8.GetOutput(7));
            _actuatorFactory.RegisterSocket(room, LivingRoom.SocketWallRightEdgeRight, hsrel8.GetOutput(4));
            _actuatorFactory.RegisterSocket(room, LivingRoom.SocketWallLeftEdgeLeft, hsrel8.GetOutput(0));
            
            _sensorFactory.RegisterButton(room, LivingRoom.ButtonUpper, input0.GetInput(15));
            _sensorFactory.RegisterButton(room, LivingRoom.ButtonMiddle, input0.GetInput(14));
            _sensorFactory.RegisterButton(room, LivingRoom.ButtonLower, input0.GetInput(13));
            _sensorFactory.RegisterButton(room, LivingRoom.ButtonPassage, input1.GetInput(10));

            room.GetLamp(LivingRoom.LampDiningTable)
                .ConnectToggleActionWith(room.GetButton(LivingRoom.ButtonUpper))
                .ConnectToggleActionWith(room.GetButton(LivingRoom.ButtonPassage));

            room.GetLamp(LivingRoom.LampCouch).
                ConnectToggleActionWith(room.GetButton(LivingRoom.ButtonMiddle));

            room.GetSocket(LivingRoom.SocketWallRightEdgeRight).
                ConnectToggleActionWith(room.GetButton(LivingRoom.ButtonLower));

            _synonymService.AddSynonymsForArea(Room.LivingRoom, "Wohnzimmer", "LivingRoom");
        }
    }
}
