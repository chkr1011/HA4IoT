using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.DHT22;
using HA4IoT.Hardware.GenericIOBoard;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class KitchenConfiguration
    {
        public enum Kitchen
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingMiddle,
            LightCeilingWall,
            LightCeilingWindow,
            LightCeilingDoor,
            LightCeilingPassageOuter,
            LightCeilingPassageInner,
            CombinedAutomaticLights,

            RollerShutter,
            RollerShutterButtons,

            ButtonPassage,
            ButtonKitchenette,

            SocketWall,
            SocketKitchenette,

            Window
        }

        public void Setup(Home home, CCToolsBoardController ccToolsController, IOBoardManager ioBoardManager, DHT22Accessor dht22Accessor)
        {
            var hsrel5 = ccToolsController.CreateHSREL5(Device.KitchenHSREL5, 58);
            var hspe8 = ccToolsController.CreateHSPE8OutputOnly(Device.KitchenHSPE8, 39);

            var input0 = ioBoardManager.GetInputBoard(Device.Input0);
            var input1 = ioBoardManager.GetInputBoard(Device.Input1);
            var input2 = ioBoardManager.GetInputBoard(Device.Input2);

            const int SensorPin = 11; // 1;

            var kitchen = home.AddRoom(Room.Kitchen)
                .WithTemperatureSensor(Kitchen.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Kitchen.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorPin))
                .WithMotionDetector(Kitchen.MotionDetector, input1.GetInput(8))
                .WithLamp(Kitchen.LightCeilingMiddle, hsrel5.GetOutput(5).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingWindow, hsrel5.GetOutput(6).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingWall, hsrel5.GetOutput(7).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingDoor, hspe8.GetOutput(0).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingPassageInner, hspe8.GetOutput(1).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingPassageOuter, hspe8.GetOutput(2).WithInvertedState())
                .WithSocket(Kitchen.SocketWall, hsrel5.GetOutput(2))
                .WithRollerShutter(Kitchen.RollerShutter, hsrel5.GetOutput(4), hsrel5.GetOutput(3), RollerShutter.DefaultMaxMovingDuration, 15467)
                .WithButton(Kitchen.ButtonKitchenette, input1.GetInput(11))
                .WithButton(Kitchen.ButtonPassage, input1.GetInput(9))
                .WithRollerShutterButtons(Kitchen.RollerShutterButtons, input2.GetInput(15), input2.GetInput(14))
                .WithWindow(Kitchen.Window, w => w.WithCenterCasement(input0.GetInput(6), input0.GetInput(7)));

            kitchen.Lamp(Kitchen.LightCeilingMiddle).ConnectToggleActionWith(kitchen.Button(Kitchen.ButtonKitchenette));
            kitchen.Lamp(Kitchen.LightCeilingMiddle).ConnectToggleActionWith(kitchen.Button(Kitchen.ButtonPassage));

            kitchen.SetupAutomaticRollerShutters().WithRollerShutter(kitchen.RollerShutter(Kitchen.RollerShutter));
            kitchen.RollerShutter(Kitchen.RollerShutter).ConnectWith(kitchen.RollerShutterButtons(Kitchen.RollerShutterButtons));

            kitchen.CombineActuators(Kitchen.CombinedAutomaticLights)
                .WithActuator(kitchen.Lamp(Kitchen.LightCeilingWall))
                .WithActuator(kitchen.Lamp(Kitchen.LightCeilingDoor))
                .WithActuator(kitchen.Lamp(Kitchen.LightCeilingWindow));

            kitchen.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(kitchen.MotionDetector(Kitchen.MotionDetector))
                .WithTarget(kitchen.BinaryStateOutput(Kitchen.CombinedAutomaticLights))
                .WithEnabledAtNight(home.WeatherStation);
        }
    }
}
