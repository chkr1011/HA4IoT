using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.DHT22;
using HA4IoT.Hardware.GenericIOBoard;

namespace HA4IoT.Controller.Main.Rooms
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

            const int SensorPin = 7;

            var childrensRoom = home.AddRoom(Room.ChildrensRoom)
                .WithTemperatureSensor(ChildrensRoom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(ChildrensRoom.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(ChildrensRoom.LightCeilingMiddle, hsrel5[HSREL5Output.GPIO2].WithInvertedState())
                .WithRollerShutter(ChildrensRoom.RollerShutter, hsrel5[HSREL5Output.Relay4], hsrel5[HSREL5Output.Relay3], RollerShutter.DefaultMaxMovingDuration, 20000)
                .WithSocket(ChildrensRoom.SocketWindow, hsrel5[HSREL5Output.Relay0])
                .WithSocket(ChildrensRoom.SocketWallLeft, hsrel5[HSREL5Output.Relay1])
                .WithSocket(ChildrensRoom.SocketWallRight, hsrel5[HSREL5Output.Relay2])
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
