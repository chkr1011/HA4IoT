using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.DHT22;
using HA4IoT.Hardware.GenericIOBoard;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class ReadingRoomConfiguration
    {
        private enum ReadingRoom
        {
            TemperatureSensor,
            HumiditySensor,

            LightCeilingMiddle,

            RollerShutter,
            RollerShutterButtons,

            Button,

            SocketWindow,
            SocketWallLeft,
            SocketWallRight,

            Window
        }

        public void Setup(IController controller, CCToolsBoardController ccToolsController, IOBoardCollection ioBoardManager, DHT22Accessor dht22Accessor)
        {
            var hsrel5 = ccToolsController.CreateHSREL5(Device.ReadingRoomHSREL5, new I2CSlaveAddress(62));
            var input2 = ioBoardManager.GetInputBoard(Device.Input2);

            const int SensorPin = 9;

            var readingRoom = controller.CreateRoom(Room.ReadingRoom)
                .WithTemperatureSensor(ReadingRoom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(ReadingRoom.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(ReadingRoom.LightCeilingMiddle, hsrel5.GetOutput(6).WithInvertedState())
                .WithRollerShutter(ReadingRoom.RollerShutter, hsrel5[HSREL5Pin.Relay4], hsrel5[HSREL5Pin.Relay3], RollerShutter.DefaultMaxMovingDuration, 20000)
                .WithSocket(ReadingRoom.SocketWindow, hsrel5[HSREL5Pin.Relay0])
                .WithSocket(ReadingRoom.SocketWallLeft, hsrel5[HSREL5Pin.Relay1])
                .WithSocket(ReadingRoom.SocketWallRight, hsrel5[HSREL5Pin.Relay2])
                .WithButton(ReadingRoom.Button, input2.GetInput(13))
                .WithWindow(ReadingRoom.Window, w => w.WithCenterCasement(input2.GetInput(8))) // Tilt = input2.GetInput(9) -- currently broken!
                .WithRollerShutterButtons(ReadingRoom.RollerShutterButtons, input2.GetInput(12), input2.GetInput(11));

            readingRoom.Lamp(ReadingRoom.LightCeilingMiddle).ConnectToggleActionWith(readingRoom.Button(ReadingRoom.Button));

            readingRoom.SetupAutomaticRollerShutters().WithRollerShutters(readingRoom.RollerShutter(ReadingRoom.RollerShutter));
            readingRoom.RollerShutter(ReadingRoom.RollerShutter)
                .ConnectWith(readingRoom.RollerShutterButtons(ReadingRoom.RollerShutterButtons));
        }
    }
}
