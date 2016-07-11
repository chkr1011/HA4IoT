using System;
using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class UpperBathroomConfiguration : RoomConfiguration
    {
        private enum UpperBathroom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingDoor,
            LightCeilingEdge,
            LightCeilingMirrorCabinet,
            LampMirrorCabinet,

            Fan,

            CombinedCeilingLights
        }

        public UpperBathroomConfiguration(IController controller) 
            : base(controller)
        {
        }

        public override void Setup()
        {
            var hsrel5 = CCToolsBoardController.CreateHSREL5(InstalledDevice.UpperBathroomHSREL5, new I2CSlaveAddress(61));
            var input5 = Controller.Device<HSPE16InputOnly>(InstalledDevice.Input5);

            const int SensorPin = 4;

            var i2cHardwareBridge = Controller.GetDevice<I2CHardwareBridge>();

            var room = Controller.CreateArea(Room.UpperBathroom)
                .WithTemperatureSensor(UpperBathroom.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(UpperBathroom.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithMotionDetector(UpperBathroom.MotionDetector, input5.GetInput(15))
                .WithLamp(UpperBathroom.LightCeilingDoor, hsrel5.GetOutput(0))
                .WithLamp(UpperBathroom.LightCeilingEdge, hsrel5.GetOutput(1))
                .WithLamp(UpperBathroom.LightCeilingMirrorCabinet, hsrel5.GetOutput(2))
                .WithLamp(UpperBathroom.LampMirrorCabinet, hsrel5.GetOutput(3))
                .WithStateMachine(UpperBathroom.Fan, (s, r) => SetupFan(s, r, hsrel5));

            var combinedLights =
                room.CombineActuators(UpperBathroom.CombinedCeilingLights)
                    .WithActuator(room.GetLamp(UpperBathroom.LightCeilingDoor))
                    .WithActuator(room.GetLamp(UpperBathroom.LightCeilingEdge))
                    .WithActuator(room.GetLamp(UpperBathroom.LightCeilingMirrorCabinet))
                    .WithActuator(room.GetLamp(UpperBathroom.LampMirrorCabinet));

            room.SetupTurnOnAndOffAutomation()
                .WithTrigger(room.GetMotionDetector(UpperBathroom.MotionDetector))
                .WithTarget(combinedLights)
                .WithOnDuration(TimeSpan.FromMinutes(8));
            
            new BathroomFanAutomation(AutomationIdFactory.CreateIdFrom<BathroomFanAutomation>(room), Controller.ServiceLocator.GetService<ISchedulerService>())
                .WithTrigger(room.GetMotionDetector(UpperBathroom.MotionDetector))
                .WithSlowDuration(TimeSpan.FromMinutes(8))
                .WithFastDuration(TimeSpan.FromMinutes(12))
                .WithActuator(room.GetStateMachine(UpperBathroom.Fan));

            Controller.ServiceLocator.GetService<SynonymService>().AddSynonymsForArea(Room.UpperBathroom, "BadOben", "UpperBathroom");
        }

        private void SetupFan(StateMachine stateMachine, IArea room, HSREL5 hsrel5)
        {
            var fanPort0 = hsrel5.GetOutput(4);
            var fanPort1 = hsrel5.GetOutput(5);

            stateMachine.AddOffState().WithOutput(fanPort0, BinaryState.Low).WithOutput(fanPort1, BinaryState.Low);
            stateMachine.AddState(new NamedComponentState("1")).WithOutput(fanPort0, BinaryState.High).WithOutput(fanPort1, BinaryState.Low);
            stateMachine.AddState(new NamedComponentState("2")).WithOutput(fanPort0, BinaryState.High).WithOutput(fanPort1, BinaryState.High);
            stateMachine.TryTurnOff();
        }
    }
}
