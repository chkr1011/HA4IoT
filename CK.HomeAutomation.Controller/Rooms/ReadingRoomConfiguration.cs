using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Connectors;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.DHT22;
using CK.HomeAutomation.Hardware.GenericIOBoard;

namespace CK.HomeAutomation.Controller.Rooms
{
    internal class ReadingRoomConfiguration
    {
        public void Setup(Home home, CCToolsBoardController ccToolsController, IOBoardManager ioBoardManager, DHT22Accessor dht22Accessor)
        {
            var hsrel5 = ccToolsController.CreateHSREL5(Device.ReadingRoomHSREL5, 62);
            var input2 = ioBoardManager.GetInputBoard(Device.Input2);

            const int SensorID = 2;

            var readingRoom = home.AddRoom(Room.ReadingRoom)
                .WithTemperatureSensor(ReadingRoom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorID))
                .WithHumiditySensor(ReadingRoom.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorID))
                .WithLamp(ReadingRoom.LightCeilingMiddle, hsrel5.GetOutput(6).WithInvertedState())
                .WithRollerShutter(ReadingRoom.RollerShutter, hsrel5.GetOutput(4), hsrel5.GetOutput(3), RollerShutter.DefaultMaxMovingDuration, 20000)
                .WithSocket(ReadingRoom.SocketWindow, hsrel5.GetOutput(0))
                .WithSocket(ReadingRoom.SocketWallLeft, hsrel5.GetOutput(1))
                .WithSocket(ReadingRoom.SocketWallRight, hsrel5.GetOutput(2))
                .WithButton(ReadingRoom.Button, input2.GetInput(13))
                .WithRollerShutterButtons(ReadingRoom.RollerShutterButtons, input2.GetInput(12), input2.GetInput(11));

            readingRoom.Lamp(ReadingRoom.LightCeilingMiddle).ConnectToggleWith(readingRoom.Button(ReadingRoom.Button));

            readingRoom.SetupAutomaticRollerShutters().WithRollerShutter(readingRoom.RollerShutter(ReadingRoom.RollerShutter));
            readingRoom.RollerShutter(ReadingRoom.RollerShutter)
                .ConnectWith(readingRoom.RollerShutterButtons(ReadingRoom.RollerShutterButtons));
        }

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
            SocketWallRight
        }
    }
}
