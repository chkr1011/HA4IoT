using System;
using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
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
    internal class LowerBathroomConfiguration : RoomConfiguration
    {
        private TimedAction _bathmodeResetTimer;

        public enum LowerBathroom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingDoor,
            LightCeilingMiddle,
            LightCeilingWindow,
            // Another light is available!
            CombinedLights,

            StartBathmodeButton,
            LampMirror,

            Window
        }

        public LowerBathroomConfiguration(IController controller, CCToolsBoardController ccToolsBoardController, RemoteSocketController remoteSocketController)
            : base(controller, ccToolsBoardController, remoteSocketController)
        {
        }

        public override void Setup()
        {
            var hspe16_FloorAndLowerBathroom = Controller.Device<HSPE16OutputOnly>(Device.LowerFloorAndLowerBathroomHSPE16);
            var input3 = Controller.Device<HSPE16InputOnly>(Device.Input3);
            var i2cHardwareBridge = Controller.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 3;

            var room = Controller.CreateArea(Room.LowerBathroom)
                .WithMotionDetector(LowerBathroom.MotionDetector, input3.GetInput(15))
                .WithTemperatureSensor(LowerBathroom.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(LowerBathroom.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(LowerBathroom.LightCeilingDoor, hspe16_FloorAndLowerBathroom.GetOutput(0).WithInvertedState())
                .WithLamp(LowerBathroom.LightCeilingMiddle, hspe16_FloorAndLowerBathroom.GetOutput(1).WithInvertedState())
                .WithLamp(LowerBathroom.LightCeilingWindow, hspe16_FloorAndLowerBathroom.GetOutput(2).WithInvertedState())
                .WithLamp(LowerBathroom.LampMirror, hspe16_FloorAndLowerBathroom.GetOutput(4).WithInvertedState())
                .WithWindow(LowerBathroom.Window, w => w.WithCenterCasement(input3.GetInput(13), input3.GetInput(14)));

            room.WithVirtualButton(LowerBathroom.StartBathmodeButton, b => b.GetPressedShortlyTrigger().Attach(() => StartBathode(room)));

            room.CombineActuators(LowerBathroom.CombinedLights)
                .WithActuator(room.GetLamp(LowerBathroom.LightCeilingDoor))
                .WithActuator(room.GetLamp(LowerBathroom.LightCeilingMiddle))
                .WithActuator(room.GetLamp(LowerBathroom.LightCeilingWindow))
                .WithActuator(room.GetLamp(LowerBathroom.LampMirror));

            room.SetupTurnOnAndOffAutomation()
                .WithTrigger(room.GetMotionDetector(LowerBathroom.MotionDetector))
                .WithTarget(room.GetStateMachine(LowerBathroom.CombinedLights));
        }

        private void StartBathode(IArea bathroom)
        {
            bathroom.GetMotionDetector().Disable();

            bathroom.GetLamp(LowerBathroom.LightCeilingDoor).TryTurnOn();
            bathroom.GetLamp(LowerBathroom.LightCeilingMiddle).TryTurnOff();
            bathroom.GetLamp(LowerBathroom.LightCeilingWindow).TryTurnOff();
            bathroom.GetLamp(LowerBathroom.LampMirror).TryTurnOff();

            _bathmodeResetTimer?.Cancel();
            _bathmodeResetTimer = bathroom.Controller.Timer.In(TimeSpan.FromHours(1)).Do(() => bathroom.GetMotionDetector().Enable());
        }
    }
}
