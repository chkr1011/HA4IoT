using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class KitchenConfiguration : RoomConfiguration
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
            RollerShutterButtonUp,
            RollerShutterButtonDown,

            ButtonPassage,
            ButtonKitchenette,

            SocketWall,
            SocketKitchenette,

            Window
        }

        public KitchenConfiguration(IController controller, CCToolsBoardController ccToolsBoardController, RemoteSocketController remoteSocketController) 
            : base(controller, ccToolsBoardController, remoteSocketController)
        {
        }

        public override void Setup()
        {
            var hsrel5 = CCToolsBoardController.CreateHSREL5(Device.KitchenHSREL5, new I2CSlaveAddress(58));
            var hspe8 = CCToolsBoardController.CreateHSPE8OutputOnly(Device.KitchenHSPE8, new I2CSlaveAddress(39));

            var input0 = Controller.Device<HSPE16InputOnly>(Device.Input0);
            var input1 = Controller.Device<HSPE16InputOnly>(Device.Input1);
            var input2 = Controller.Device<HSPE16InputOnly>(Device.Input2);

            var i2cHardwareBridge = Controller.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 11;

            var room = Controller.CreateArea(Room.Kitchen)
                .WithTemperatureSensor(Kitchen.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Kitchen.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithMotionDetector(Kitchen.MotionDetector, input1.GetInput(8))
                .WithLamp(Kitchen.LightCeilingMiddle, hsrel5.GetOutput(5).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingWindow, hsrel5.GetOutput(6).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingWall, hsrel5.GetOutput(7).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingDoor, hspe8.GetOutput(0).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingPassageInner, hspe8.GetOutput(1).WithInvertedState())
                .WithLamp(Kitchen.LightCeilingPassageOuter, hspe8.GetOutput(2).WithInvertedState())
                .WithSocket(Kitchen.SocketWall, hsrel5.GetOutput(2))
                .WithRollerShutter(Kitchen.RollerShutter, hsrel5.GetOutput(4), hsrel5.GetOutput(3))
                .WithButton(Kitchen.ButtonKitchenette, input1.GetInput(11))
                .WithButton(Kitchen.ButtonPassage, input1.GetInput(9))
                .WithRollerShutterButtons(Kitchen.RollerShutterButtonUp, input2.GetInput(15), Kitchen.RollerShutterButtonDown, input2.GetInput(14))
                .WithWindow(Kitchen.Window, w => w.WithCenterCasement(input0.GetInput(6), input0.GetInput(7)));

            room.GetLamp(Kitchen.LightCeilingMiddle).ConnectToggleActionWith(room.GetButton(Kitchen.ButtonKitchenette));
            room.GetLamp(Kitchen.LightCeilingMiddle).ConnectToggleActionWith(room.GetButton(Kitchen.ButtonPassage));

            room.SetupRollerShutterAutomation().WithRollerShutters(room.GetRollerShutter(Kitchen.RollerShutter));

            room.GetRollerShutter(Kitchen.RollerShutter).ConnectWith(
                room.GetButton(Kitchen.RollerShutterButtonUp), room.GetButton(Kitchen.RollerShutterButtonDown));

            room.CombineActuators(Kitchen.CombinedAutomaticLights)
                .WithActuator(room.GetLamp(Kitchen.LightCeilingWall))
                .WithActuator(room.GetLamp(Kitchen.LightCeilingDoor))
                .WithActuator(room.GetLamp(Kitchen.LightCeilingWindow));

            room.SetupTurnOnAndOffAutomation()
                .WithTrigger(room.GetMotionDetector(Kitchen.MotionDetector))
                .WithTarget(room.GetStateMachine(Kitchen.CombinedAutomaticLights))
                .WithEnabledAtNight(Controller.GetService<IDaylightService>());
        }
    }
}
