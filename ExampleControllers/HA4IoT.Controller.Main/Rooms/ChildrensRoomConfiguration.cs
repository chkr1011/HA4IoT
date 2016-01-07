using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.DHT22;

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

        public void Setup(Controller controller, CCToolsBoardController ccToolsController, DHT22Accessor dht22Accessor)
        {
            var hsrel5 = ccToolsController.CreateHSREL5(Device.ChildrensRoomHSREL5, new I2CSlaveAddress(63));
            var input0 = controller.GetDevice<HSPE16InputOnly>(Device.Input0);

            const int SensorPin = 7;

            var childrensRoom = controller.CreateRoom(Room.ChildrensRoom)
                .WithTemperatureSensor(ChildrensRoom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(ChildrensRoom.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(ChildrensRoom.LightCeilingMiddle, hsrel5[HSREL5Pin.GPIO1].WithInvertedState())
                .WithRollerShutter(ChildrensRoom.RollerShutter, hsrel5[HSREL5Pin.Relay4], hsrel5[HSREL5Pin.Relay3], RollerShutter.DefaultMaxMovingDuration, 20000)
                .WithSocket(ChildrensRoom.SocketWindow, hsrel5[HSREL5Pin.Relay0])
                .WithSocket(ChildrensRoom.SocketWallLeft, hsrel5[HSREL5Pin.Relay1])
                .WithSocket(ChildrensRoom.SocketWallRight, hsrel5[HSREL5Pin.Relay2])
                .WithButton(ChildrensRoom.Button, input0.GetInput(0))
                .WithWindow(ChildrensRoom.Window, w => w.WithCenterCasement(input0.GetInput(5), input0.GetInput(4)))
                .WithRollerShutterButtons(ChildrensRoom.RollerShutterButtons, input0.GetInput(1), input0.GetInput(2));

            childrensRoom.Lamp(ChildrensRoom.LightCeilingMiddle).ConnectToggleActionWith(childrensRoom.Button(ChildrensRoom.Button));

            childrensRoom.SetupAutomaticRollerShutters().WithRollerShutters(childrensRoom.RollerShutter(ChildrensRoom.RollerShutter));
            childrensRoom.RollerShutter(ChildrensRoom.RollerShutter)
                .ConnectWith(childrensRoom.RollerShutterButtons(ChildrensRoom.RollerShutterButtons));
        }
    }
}
