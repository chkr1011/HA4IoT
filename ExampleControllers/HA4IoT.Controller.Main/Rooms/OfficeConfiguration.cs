using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class OfficeConfiguration : RoomConfiguration
    {
        public enum Office
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeilingFrontLeft,
            LightCeilingFrontMiddle,
            LightCeilingFrontRight,
            LightCeilingMiddleLeft,
            LightCeilingMiddleMiddle,
            LightCeilingMiddleRight,
            LightCeilingRearLeft,
            LightCeilingRearRight,

            SocketFrontLeft,
            SocketFrontRight,
            SocketWindowLeft,
            SocketWindowRight,
            SocketRearRight,
            SocketRearLeft,
            SocketRearLeftEdge,
            RemoteSocketDesk,

            ButtonUpperLeft,
            ButtonUpperRight,
            ButtonLowerLeft,
            ButtonLowerRight,

            CombinedCeilingLights,
            CombinedCeilingLightsCouchOnly,
            CombinedCeilingLightsDeskOnly,
            CombinedCeilingLightsOther,

            WindowLeft,
            WindowRight
        }

        public OfficeConfiguration(IController controller) 
            : base(controller)
        {
        }

        public override void Setup()
        {
            var hsrel8 = CCToolsBoardController.CreateHSREL8(Device.OfficeHSREL8, new I2CSlaveAddress(20));
            var hspe8 = CCToolsBoardController.CreateHSPE8OutputOnly(Device.UpperFloorAndOfficeHSPE8, new I2CSlaveAddress(37));
            var input4 = Controller.Device<HSPE16InputOnly>(Device.Input4);
            var input5 = Controller.Device<HSPE16InputOnly>(Device.Input5);

            const int SensorPin = 2;

            var i2cHardwareBridge = Controller.GetDevice<I2CHardwareBridge>();

            var room = Controller.CreateArea(Room.Office)
                .WithMotionDetector(Office.MotionDetector, input4.GetInput(13))
                .WithTemperatureSensor(Office.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Office.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithLamp(Office.LightCeilingFrontRight, hsrel8.GetOutput(8).WithInvertedState())
                .WithLamp(Office.LightCeilingFrontMiddle, hspe8.GetOutput(2).WithInvertedState())
                .WithLamp(Office.LightCeilingFrontLeft, hspe8.GetOutput(0).WithInvertedState())
                .WithLamp(Office.LightCeilingMiddleRight, hsrel8.GetOutput(9).WithInvertedState())
                .WithLamp(Office.LightCeilingMiddleMiddle, hspe8.GetOutput(3).WithInvertedState())
                .WithLamp(Office.LightCeilingMiddleLeft, hspe8.GetOutput(1).WithInvertedState())
                .WithLamp(Office.LightCeilingRearRight, hsrel8.GetOutput(12).WithInvertedState())
                .WithLamp(Office.LightCeilingRearLeft, hsrel8.GetOutput(13).WithInvertedState())
                .WithSocket(Office.SocketFrontLeft, hsrel8.GetOutput(0))
                .WithSocket(Office.SocketFrontRight, hsrel8.GetOutput(6))
                .WithSocket(Office.SocketWindowLeft, hsrel8.GetOutput(10).WithInvertedState())
                .WithSocket(Office.SocketWindowRight, hsrel8.GetOutput(11).WithInvertedState())
                .WithSocket(Office.SocketRearLeftEdge, hsrel8.GetOutput(7))
                .WithSocket(Office.SocketRearLeft, hsrel8.GetOutput(2))
                .WithSocket(Office.SocketRearRight, hsrel8.GetOutput(1))
                .WithButton(Office.ButtonUpperLeft, input5.GetInput(0))
                .WithButton(Office.ButtonLowerLeft, input5.GetInput(1))
                .WithButton(Office.ButtonLowerRight, input4.GetInput(14))
                .WithButton(Office.ButtonUpperRight, input4.GetInput(15))
                .WithWindow(Office.WindowLeft, w => w.WithLeftCasement(input4.GetInput(11)).WithRightCasement(input4.GetInput(12), input4.GetInput(10)))
                .WithWindow(Office.WindowRight, w => w.WithLeftCasement(input4.GetInput(8)).WithRightCasement(input4.GetInput(9), input5.GetInput(8)))
                .WithSocket(Office.RemoteSocketDesk, RemoteSocketController.GetOutput(0))
                .WithStateMachine(Office.CombinedCeilingLights, SetupLight);
            
            room.GetButton(Office.ButtonUpperLeft).GetPressedLongTrigger().Attach(() =>
            {
                room.GetStateMachine(Office.CombinedCeilingLights).TryTurnOff();
                room.Socket(Office.SocketRearLeftEdge).TryTurnOff();
                room.Socket(Office.SocketRearLeft).TryTurnOff();
                room.Socket(Office.SocketFrontLeft).TryTurnOff();
            });
        }

        private void SetupLight(StateMachine light, IArea room)
        {
            var lightsCouchOnly = room.CombineActuators(Office.CombinedCeilingLightsCouchOnly)
                .WithActuator(room.GetLamp(Office.LightCeilingRearRight));

            var lightsDeskOnly = room.CombineActuators(Office.CombinedCeilingLightsDeskOnly)
                .WithActuator(room.GetLamp(Office.LightCeilingFrontMiddle))
                .WithActuator(room.GetLamp(Office.LightCeilingFrontLeft))
                .WithActuator(room.GetLamp(Office.LightCeilingMiddleLeft));

            var lightsOther = room.CombineActuators(Office.CombinedCeilingLightsOther)
                .WithActuator(room.GetLamp(Office.LightCeilingFrontRight))
                .WithActuator(room.GetLamp(Office.LightCeilingMiddleMiddle))
                .WithActuator(room.GetLamp(Office.LightCeilingMiddleRight))
                .WithActuator(room.GetLamp(Office.LightCeilingRearLeft));

            light.WithTurnOffIfStateIsAppliedTwice();

            light.AddOffState()
                .WithActuator(lightsDeskOnly, BinaryStateId.Off)
                .WithActuator(lightsCouchOnly, BinaryStateId.Off)
                .WithActuator(lightsOther, BinaryStateId.Off);

            light.AddOnState()
                .WithActuator(lightsDeskOnly, BinaryStateId.On)
                .WithActuator(lightsCouchOnly, BinaryStateId.On)
                .WithActuator(lightsOther, BinaryStateId.On);

            var deskOnlyStateId = new StatefulComponentState("DeskOnly");
            light.AddState(deskOnlyStateId)
                .WithActuator(lightsDeskOnly, BinaryStateId.On)
                .WithActuator(lightsCouchOnly, BinaryStateId.Off)
                .WithActuator(lightsOther, BinaryStateId.Off);

            var couchOnlyStateId = new StatefulComponentState("CouchOnly");
            light.AddState(couchOnlyStateId)
                .WithActuator(lightsDeskOnly, BinaryStateId.Off)
                .WithActuator(lightsCouchOnly, BinaryStateId.On)
                .WithActuator(lightsOther, BinaryStateId.Off);

            room.GetButton(Office.ButtonLowerRight)
                .GetPressedShortlyTrigger()
                .OnTriggered(light.GetSetStateAction(couchOnlyStateId));

            room.GetButton(Office.ButtonLowerLeft)
                .GetPressedShortlyTrigger()
                .OnTriggered(light.GetSetStateAction(deskOnlyStateId));

            room.GetButton(Office.ButtonUpperLeft)
                .GetPressedShortlyTrigger()
                .OnTriggered(light.GetSetStateAction(BinaryStateId.On));
        }
    }
}
