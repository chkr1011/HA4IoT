using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Connectors;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.Drivers;

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
            SocketWallRight
        }

        public void Setup(Home home, IOBoardManager ioBoardManager, CCToolsFactory ccToolsFactory, TemperatureAndHumiditySensorBridgeDriver sensorBridgeDriver)
        {
            var hsrel5 = ccToolsFactory.CreateHSREL5(Device.ChildrensRoomHSREL5, 63);
            var input0 = ioBoardManager.GetInputBoard(Device.Input0);

            var childrensRoom = home.AddRoom(Room.ChildrensRoom)
                .WithTemperatureSensor(ChildrensRoom.TemperatureSensor, 3, sensorBridgeDriver)
                .WithHumiditySensor(ChildrensRoom.HumiditySensor, 3, sensorBridgeDriver)
                .WithLamp(ChildrensRoom.LightCeilingMiddle, hsrel5.GetOutput(6).WithInvertedState())
                .WithRollerShutter(ChildrensRoom.RollerShutter, hsrel5.GetOutput(4), hsrel5.GetOutput(3), RollerShutter.DefaultMaxMovingDuration)
                .WithSocket(ChildrensRoom.SocketWindow, hsrel5.GetOutput(0))
                .WithSocket(ChildrensRoom.SocketWallLeft, hsrel5.GetOutput(1))
                .WithSocket(ChildrensRoom.SocketWallRight, hsrel5.GetOutput(2))
                .WithButton(ChildrensRoom.Button, input0.GetInput(0))
                .WithRollerShutterButtons(ChildrensRoom.RollerShutterButtons, input0.GetInput(1), input0.GetInput(2));

            childrensRoom.Lamp(ChildrensRoom.LightCeilingMiddle).ConnectToggleWith(childrensRoom.Button(ChildrensRoom.Button));

            childrensRoom.SetupAutomaticRollerShutters().WithRollerShutter(childrensRoom.RollerShutter(ChildrensRoom.RollerShutter));
            childrensRoom.RollerShutter(ChildrensRoom.RollerShutter)
                .ConnectWith(childrensRoom.RollerShutterButtons(ChildrensRoom.RollerShutterButtons));
        }
    }
}
