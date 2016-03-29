using HA4IoT.Actuators;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.RemoteSwitch;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class OfficeConfiguration
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

        public void Setup(Controller controller, CCToolsBoardController ccToolsController, RemoteSocketController remoteSwitchController)
        {
            var hsrel8 = ccToolsController.CreateHSREL8(Device.OfficeHSREL8, new I2CSlaveAddress(20));
            var hspe8 = ccToolsController.CreateHSPE8OutputOnly(Device.UpperFloorAndOfficeHSPE8, new I2CSlaveAddress(37));
            var input4 = controller.Device<HSPE16InputOnly>(Device.Input4);
            var input5 = controller.Device<HSPE16InputOnly>(Device.Input5);

            const int SensorPin = 2;

            var i2cHardwareBridge = controller.GetDevice<I2CHardwareBridge>();

            var office = controller.CreateArea(Room.Office)
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
                .WithSocket(Office.RemoteSocketDesk, remoteSwitchController.GetOutput(0))
                .WithStateMachine(Office.CombinedCeilingLights, SetupLight);
            
            office.GetButton(Office.ButtonUpperLeft).GetPressedLongTrigger().Attach(() =>
            {
                office.GetStateMachine(Office.CombinedCeilingLights).TryTurnOff();
                office.Socket(Office.SocketRearLeftEdge).TryTurnOff();
                office.Socket(Office.SocketRearLeft).TryTurnOff();
                office.Socket(Office.SocketFrontLeft).TryTurnOff();
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
                .WithActuator(lightsDeskOnly, DefaultStateIDs.Off)
                .WithActuator(lightsCouchOnly, DefaultStateIDs.Off)
                .WithActuator(lightsOther, DefaultStateIDs.Off);

            light.AddOnState()
                .WithActuator(lightsDeskOnly, DefaultStateIDs.On)
                .WithActuator(lightsCouchOnly, DefaultStateIDs.On)
                .WithActuator(lightsOther, DefaultStateIDs.On);

            var deskOnlyStateId = new StateMachineStateId("DeskOnly");
            light.AddState(deskOnlyStateId)
                .WithActuator(lightsDeskOnly, DefaultStateIDs.On)
                .WithActuator(lightsCouchOnly, DefaultStateIDs.Off)
                .WithActuator(lightsOther, DefaultStateIDs.Off);

            var couchOnlyStateId = new StateMachineStateId("CouchOnly");
            light.AddState(couchOnlyStateId)
                .WithActuator(lightsDeskOnly, DefaultStateIDs.Off)
                .WithActuator(lightsCouchOnly, DefaultStateIDs.On)
                .WithActuator(lightsOther, DefaultStateIDs.Off);

            room.GetButton(Office.ButtonLowerRight)
                .GetPressedShortlyTrigger()
                .OnTriggered(light.GetSetActiveStateAction(couchOnlyStateId));

            room.GetButton(Office.ButtonLowerLeft)
                .GetPressedShortlyTrigger()
                .OnTriggered(light.GetSetActiveStateAction(deskOnlyStateId));

            room.GetButton(Office.ButtonUpperLeft)
                .GetPressedShortlyTrigger()
                .OnTriggered(light.GetSetActiveStateAction(DefaultStateIDs.On));
        }
    }
}
