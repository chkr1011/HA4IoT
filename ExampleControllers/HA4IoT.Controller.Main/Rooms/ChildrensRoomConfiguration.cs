using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Automations;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;

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

        public void Setup(Controller controller, CCToolsBoardController ccToolsController)
        {
            var hsrel5 = ccToolsController.CreateHSREL5(Device.ChildrensRoomHSREL5, new I2CSlaveAddress(63));
            var input0 = controller.Device<HSPE16InputOnly>(Device.Input0);

            var i2cHardwareBridge = controller.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 7;

            var childrensRoom = controller.CreateArea(Room.ChildrensRoom)
                .WithTemperatureSensor(ChildrensRoom.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(ChildrensRoom.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(ChildrensRoom.LightCeilingMiddle, hsrel5[HSREL5Pin.GPIO1].WithInvertedState())
                .WithRollerShutter(ChildrensRoom.RollerShutter, hsrel5[HSREL5Pin.Relay4], hsrel5[HSREL5Pin.Relay3])
                .WithSocket(ChildrensRoom.SocketWindow, hsrel5[HSREL5Pin.Relay0])
                .WithSocket(ChildrensRoom.SocketWallLeft, hsrel5[HSREL5Pin.Relay1])
                .WithSocket(ChildrensRoom.SocketWallRight, hsrel5[HSREL5Pin.Relay2])
                .WithButton(ChildrensRoom.Button, input0.GetInput(0))
                .WithWindow(ChildrensRoom.Window, w => w.WithCenterCasement(input0.GetInput(5), input0.GetInput(4)))
                .WithRollerShutterButtons(ChildrensRoom.RollerShutterButtons, input0.GetInput(1), input0.GetInput(2));

            childrensRoom.GetLamp(ChildrensRoom.LightCeilingMiddle).ConnectToggleActionWith(childrensRoom.GetButton(ChildrensRoom.Button));

            childrensRoom.SetupRollerShutterAutomation().WithRollerShutters(childrensRoom.GetRollerShutter(ChildrensRoom.RollerShutter));
            childrensRoom.GetRollerShutter(ChildrensRoom.RollerShutter)
                .ConnectWith(childrensRoom.GetRollerShutterButtons(ChildrensRoom.RollerShutterButtons));
        }
    }
}
