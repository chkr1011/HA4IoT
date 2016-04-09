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
    internal class ChildrensRoomRoomConfiguration : RoomConfiguration
    {
        private enum ChildrensRoom
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

        public ChildrensRoomRoomConfiguration(IController controller) 
            : base(controller)
        {
        }

        public override void Setup()
        {
            var hsrel5 = CCToolsBoardController.CreateHSREL5(InstalledDevice.ChildrensRoomHSREL5, new I2CSlaveAddress(63));
            var input0 = Controller.Device<HSPE16InputOnly>(InstalledDevice.Input0);

            var i2cHardwareBridge = Controller.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 7;

            var room = Controller.CreateArea(Room.ChildrensRoom)
                .WithTemperatureSensor(ChildrensRoom.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(ChildrensRoom.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(ChildrensRoom.LightCeilingMiddle, hsrel5[HSREL5Pin.GPIO1].WithInvertedState())
                .WithRollerShutter(ChildrensRoom.RollerShutter, hsrel5[HSREL5Pin.Relay4], hsrel5[HSREL5Pin.Relay3])
                .WithSocket(ChildrensRoom.SocketWindow, hsrel5[HSREL5Pin.Relay0])
                .WithSocket(ChildrensRoom.SocketWallLeft, hsrel5[HSREL5Pin.Relay1])
                .WithSocket(ChildrensRoom.SocketWallRight, hsrel5[HSREL5Pin.Relay2])
                .WithButton(ChildrensRoom.Button, input0.GetInput(0))
                .WithWindow(ChildrensRoom.Window, w => w.WithCenterCasement(input0.GetInput(5), input0.GetInput(4)))
                .WithRollerShutterButtons(ChildrensRoom.RollerShutterButtonUp, input0.GetInput(1), ChildrensRoom.RollerShutterButtonDown, input0.GetInput(2));

            room.GetLamp(ChildrensRoom.LightCeilingMiddle).ConnectToggleActionWith(room.GetButton(ChildrensRoom.Button));

            room.SetupRollerShutterAutomation().WithRollerShutters(room.GetRollerShutter(ChildrensRoom.RollerShutter));
            room.GetRollerShutter(ChildrensRoom.RollerShutter)
                .ConnectWith(room.GetButton(ChildrensRoom.RollerShutterButtonUp), room.GetButton(ChildrensRoom.RollerShutterButtonDown));
        }
    }
}
