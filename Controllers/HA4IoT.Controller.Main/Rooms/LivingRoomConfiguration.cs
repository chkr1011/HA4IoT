using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
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
    internal class LivingRoomConfiguration
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly SynonymService _synonymService;

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
            SynonymService synonymService)
        {
            _deviceService = deviceService;
            _areaService = areaService;
            _ccToolsBoardService = ccToolsBoardService;
            _synonymService = synonymService;
        }

        public void Setup()
        {
            var hsrel8 = _ccToolsBoardService.CreateHSREL8(InstalledDevice.LivingRoomHSREL8, new I2CSlaveAddress(18));
            var hsrel5 = _ccToolsBoardService.CreateHSREL5(InstalledDevice.LivingRoomHSREL5, new I2CSlaveAddress(57));
            
            var input0 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input0);
            var input1 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input1);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 12;

            var room = _areaService.CreateArea(Room.LivingRoom)
                .WithTemperatureSensor(LivingRoom.TemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(LivingRoom.HumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(LivingRoom.LampCouch, hsrel8.GetOutput(8).WithInvertedState())
                .WithLamp(LivingRoom.LampDiningTable, hsrel8.GetOutput(9).WithInvertedState())
                .WithSocket(LivingRoom.SocketWindowLeftLower, hsrel8.GetOutput(1))
                .WithSocket(LivingRoom.SocketWindowMiddleLower, hsrel8.GetOutput(2))
                .WithSocket(LivingRoom.SocketWindowRightLower, hsrel8.GetOutput(3))
                .WithSocket(LivingRoom.SocketWindowLeftUpper, hsrel8.GetOutput(5))
                .WithSocket(LivingRoom.SocketWindowRightUpper, hsrel8.GetOutput(7))

                .WithSocket(LivingRoom.SocketWallRightEdgeRight, hsrel8.GetOutput(4))

                .WithSocket(LivingRoom.SocketWallLeftEdgeLeft, hsrel8.GetOutput(0))

                .WithButton(LivingRoom.ButtonUpper, input0.GetInput(15))
                .WithButton(LivingRoom.ButtonMiddle, input0.GetInput(14))
                .WithButton(LivingRoom.ButtonLower, input0.GetInput(13))
                .WithButton(LivingRoom.ButtonPassage, input1.GetInput(10))
                .WithWindow(LivingRoom.WindowLeft,
                    w => w.WithLeftCasement(input0.GetInput(10), input0.GetInput(11)).WithRightCasement(input0.GetInput(9), input0.GetInput(8)))
                .WithWindow(LivingRoom.WindowRight,
                    w => w.WithLeftCasement(input1.GetInput(14), input1.GetInput(15)).WithRightCasement(input1.GetInput(13), input1.GetInput(12)));

            room.GetLamp(LivingRoom.LampDiningTable)
                .ConnectToggleActionWith(room.GetButton(LivingRoom.ButtonUpper))
                .ConnectToggleActionWith(room.GetButton(LivingRoom.ButtonPassage));

            room.GetLamp(LivingRoom.LampCouch).
                ConnectToggleActionWith(room.GetButton(LivingRoom.ButtonMiddle));

            room.Socket(LivingRoom.SocketWallRightEdgeRight).
                ConnectToggleActionWith(room.GetButton(LivingRoom.ButtonLower));

            _synonymService.AddSynonymsForArea(Room.LivingRoom, "Wohnzimmer", "LivingRoom");
        }
    }
}
