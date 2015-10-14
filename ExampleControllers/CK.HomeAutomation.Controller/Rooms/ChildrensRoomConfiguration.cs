using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Connectors;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.DHT22;
using CK.HomeAutomation.Hardware.GenericIOBoard;

namespace CK.HomeAutomation.Controller.Rooms
{
    internal class ChildrensRoomRoomConfiguration
    {
        private enum ChildrensRoom
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

        public void Setup(Home home, CCToolsBoardController ccToolsController, IOBoardManager ioBoardManager, DHT22Accessor dht22Accessor)
        {
            var hsrel5 = ccToolsController.CreateHSREL5(Device.ChildrensRoomHSREL5, 63);
            var input0 = ioBoardManager.GetInputBoard(Device.Input0);

            const int SensorID = 3;

            var childrensRoom = home.AddRoom(Room.ChildrensRoom)
                .WithTemperatureSensor(ChildrensRoom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorID))
                .WithHumiditySensor(ChildrensRoom.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorID))
                .WithLamp(ChildrensRoom.LightCeilingMiddle, hsrel5.GetOutput(6).WithInvertedState())
                .WithRollerShutter(ChildrensRoom.RollerShutter, hsrel5.GetOutput(4), hsrel5.GetOutput(3), RollerShutter.DefaultMaxMovingDuration, 20000)
                .WithSocket(ChildrensRoom.SocketWindow, hsrel5.GetOutput(0))
                .WithSocket(ChildrensRoom.SocketWallLeft, hsrel5.GetOutput(1))
                .WithSocket(ChildrensRoom.SocketWallRight, hsrel5.GetOutput(2))
                .WithButton(ChildrensRoom.Button, input0.GetInput(0))
                .WithWindow(ChildrensRoom.Window, w => w.WithCenterCasement(input0.GetInput(5), input0.GetInput(4)))
                .WithRollerShutterButtons(ChildrensRoom.RollerShutterButtons, input0.GetInput(1), input0.GetInput(2));

            childrensRoom.Lamp(ChildrensRoom.LightCeilingMiddle).ConnectToggleActionWith(childrensRoom.Button(ChildrensRoom.Button));

            childrensRoom.SetupAutomaticRollerShutters().WithRollerShutter(childrensRoom.RollerShutter(ChildrensRoom.RollerShutter));
            childrensRoom.RollerShutter(ChildrensRoom.RollerShutter)
                .ConnectWith(childrensRoom.RollerShutterButtons(ChildrensRoom.RollerShutterButtons));
        }
    }
}
