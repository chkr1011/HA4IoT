using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Connectors;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.DHT22;
using CK.HomeAutomation.Hardware.GenericIOBoard;

namespace CK.HomeAutomation.Controller.Rooms
{
    internal class LivingRoomConfiguration
    {
        public enum LivingRoom
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

            ButtonUpper,
            ButtonMiddle,
            ButtonLower,
            ButtonPassage,

            WindowLeft,
            WindowRight,
        }

        public void Setup(Home home, CCToolsBoardController ccToolsController, IOBoardManager ioBoardManager, DHT22Accessor dht22Accessor)
        {
            var hsrel8 = ccToolsController.CreateHSREL8(Device.LivingRoomHSREL8, 18);
            var hsrel5 = ccToolsController.CreateHSREL5(Device.LivingRoomHSREL5, 57);
            
            var input0 = ioBoardManager.GetInputBoard(Device.Input0);
            var input1 = ioBoardManager.GetInputBoard(Device.Input1);

            const int SensorID = 0;

            var livingRoom = home.AddRoom(Room.LivingRoom)
                .WithTemperatureSensor(LivingRoom.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorID))
                .WithHumiditySensor(LivingRoom.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorID))
                .WithLamp(LivingRoom.LampCouch, hsrel8.GetOutput(8).WithInvertedState())
                .WithLamp(LivingRoom.LampDiningTable, hsrel8.GetOutput(9).WithInvertedState())
                .WithSocket(LivingRoom.SocketWindowLeftLower, hsrel8.GetOutput(1))
                .WithSocket(LivingRoom.SocketWindowMiddleLower, hsrel8.GetOutput(2))
                .WithSocket(LivingRoom.SocketWindowRightLower, hsrel8.GetOutput(3))
                .WithSocket(LivingRoom.SocketWallRightEdgeRight, hsrel8.GetOutput(4))
                .WithSocket(LivingRoom.SocketWindowLeftUpper, hsrel8.GetOutput(5))
                .WithSocket(LivingRoom.SocketWindowRightUpper, hsrel8.GetOutput(7))
                .WithButton(LivingRoom.ButtonUpper, input0.GetInput(15))
                .WithButton(LivingRoom.ButtonMiddle, input0.GetInput(14))
                .WithButton(LivingRoom.ButtonLower, input0.GetInput(13))
                .WithButton(LivingRoom.ButtonPassage, input1.GetInput(10))
                .WithWindow(LivingRoom.WindowLeft,
                    w => w.WithCasement(input0.GetInput(14), input0.GetInput(15)).WithCasement(input0.GetInput(13), input0.GetInput(12)));

            livingRoom.Lamp(LivingRoom.LampDiningTable)
                .ConnectToggleWith(livingRoom.Button(LivingRoom.ButtonUpper))
                .ConnectToggleWith(livingRoom.Button(LivingRoom.ButtonPassage));

            livingRoom.Lamp(LivingRoom.LampCouch).
                ConnectToggleWith(livingRoom.Button(LivingRoom.ButtonMiddle));

            livingRoom.Socket(LivingRoom.SocketWallRightEdgeRight).
                ConnectToggleWith(livingRoom.Button(LivingRoom.ButtonLower));
        }
    }
}
