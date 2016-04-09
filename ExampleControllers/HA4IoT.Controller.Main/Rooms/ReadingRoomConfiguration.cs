using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class ReadingRoomConfiguration : RoomConfiguration
    {
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

        public ReadingRoomConfiguration(IController controller) 
            : base(controller)
        {
        }

        public override void Setup()
        {
            var hsrel5 = CCToolsBoardController.CreateHSREL5(InstalledDevice.ReadingRoomHSREL5, new I2CSlaveAddress(62));
            var input2 = Controller.Device<HSPE16InputOnly>(InstalledDevice.Input2);

            var i2cHardwareBridge = Controller.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 9;

            var room = Controller.CreateArea(Room.ReadingRoom)
                .WithTemperatureSensor(ReadingRoom.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(ReadingRoom.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(ReadingRoom.LightCeilingMiddle, hsrel5.GetOutput(6).WithInvertedState())
                .WithRollerShutter(ReadingRoom.RollerShutter, hsrel5[HSREL5Pin.Relay4], hsrel5[HSREL5Pin.Relay3])
                .WithSocket(ReadingRoom.SocketWindow, hsrel5[HSREL5Pin.Relay0])
                .WithSocket(ReadingRoom.SocketWallLeft, hsrel5[HSREL5Pin.Relay1])
                .WithSocket(ReadingRoom.SocketWallRight, hsrel5[HSREL5Pin.Relay2])
                .WithButton(ReadingRoom.Button, input2.GetInput(13))
                .WithWindow(ReadingRoom.Window, w => w.WithCenterCasement(input2.GetInput(8))) // Tilt = input2.GetInput(9) -- currently broken!
                .WithRollerShutterButtons(ReadingRoom.RollerShutterButtonUp, input2.GetInput(12), ReadingRoom.RollerShutterButtonDown, input2.GetInput(11));

            room.GetLamp(ReadingRoom.LightCeilingMiddle).ConnectToggleActionWith(room.GetButton(ReadingRoom.Button));

            room.SetupRollerShutterAutomation().WithRollerShutters(room.GetRollerShutter(ReadingRoom.RollerShutter));
            room.GetRollerShutter(ReadingRoom.RollerShutter)
                .ConnectWith(room.GetButton(ReadingRoom.RollerShutterButtonUp), room.GetButton(ReadingRoom.RollerShutterButtonDown));
        }
    }
}
