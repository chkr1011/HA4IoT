using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.DHT22;
using CK.HomeAutomation.Hardware.GenericIOBoard;
using CK.HomeAutomation.Hardware.RemoteSwitch;

namespace CK.HomeAutomation.Controller.Rooms
{
    internal class OfficeConfiguration
    {
        private enum Office
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

        public void Setup(Home home, CCToolsBoardController ccToolsController, IOBoardManager ioBoardManager, DHT22Accessor dht22Accessor, RemoteSwitchController remoteSwitchController)
        {
            var hsrel8 = ccToolsController.CreateHSREL8(Device.OfficeHSREL8, 20);
            var hspe8 = ccToolsController.CreateHSPE8OutputOnly(Device.UpperFloorAndOfficeHSPE8, 37);
            var input4 = ioBoardManager.GetInputBoard(Device.Input4);
            var input5 = ioBoardManager.GetInputBoard(Device.Input5);

            const int SensorPin = 2; //6;

            var office = home.AddRoom(Room.Office)
                .WithMotionDetector(Office.MotionDetector, input4.GetInput(13))
                .WithTemperatureSensor(Office.TemperatureSensor, dht22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Office.HumiditySensor, dht22Accessor.GetHumiditySensor(SensorPin))
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
                .WithWindow(Office.WindowRight, w => w.WithLeftCasement(input4.GetInput(8)).WithRightCasement(input4.GetInput(9), input5.GetInput(8)));

            office.WithSocket(Office.RemoteSocketDesk, remoteSwitchController.GetOutput(0));

            var lightsCouchOnly = office.CombineActuators(Office.CombinedCeilingLightsCouchOnly)
                .WithActuator(office.Actuator<Lamp>(Office.LightCeilingRearRight));

            var lightsDeskOnly = office.CombineActuators(Office.CombinedCeilingLightsDeskOnly)
                .WithActuator(office.Actuator<Lamp>(Office.LightCeilingFrontMiddle))
                .WithActuator(office.Actuator<Lamp>(Office.LightCeilingFrontLeft))
                .WithActuator(office.Actuator<Lamp>(Office.LightCeilingMiddleLeft));

            var lightsOther = office.CombineActuators(Office.CombinedCeilingLightsOther)
                .WithMaster(office.Actuator<Lamp>(Office.LightCeilingFrontRight))
                .WithActuator(office.Actuator<Lamp>(Office.LightCeilingMiddleMiddle))
                .WithActuator(office.Actuator<Lamp>(Office.LightCeilingMiddleRight))
                .WithActuator(office.Actuator<Lamp>(Office.LightCeilingRearLeft));

            var light = office.AddStateMachine(Office.CombinedCeilingLights)
                .WithTurnOffIfStateIsAppliedTwice();

            light.AddOffState()
                .WithActuator(lightsDeskOnly, BinaryActuatorState.Off)
                .WithActuator(lightsCouchOnly, BinaryActuatorState.Off)
                .WithActuator(lightsOther, BinaryActuatorState.Off);

            light.AddOnState()
                .WithActuator(lightsDeskOnly, BinaryActuatorState.On)
                .WithActuator(lightsCouchOnly, BinaryActuatorState.On)
                .WithActuator(lightsOther, BinaryActuatorState.On).
                ConnectApplyStateWith(office.Actuator<Button>(Office.ButtonUpperLeft));

            light.AddState("DeskOnly")
                .WithActuator(lightsDeskOnly, BinaryActuatorState.On)
                .WithActuator(lightsCouchOnly, BinaryActuatorState.Off)
                .WithActuator(lightsOther, BinaryActuatorState.Off)
                .ConnectApplyStateWith(office.Actuator<Button>(Office.ButtonLowerLeft));

            light.AddState("CouchOnly")
                .WithActuator(lightsDeskOnly, BinaryActuatorState.Off)
                .WithActuator(lightsCouchOnly, BinaryActuatorState.On)
                .WithActuator(lightsOther, BinaryActuatorState.Off)
                .ConnectApplyStateWith(office.Actuator<Button>(Office.ButtonLowerRight));

            office.Button(Office.ButtonUpperLeft).WithLongAction(() =>
            {
                light.TurnOff();
                office.Socket(Office.SocketRearLeftEdge).TurnOff();
                office.Socket(Office.SocketRearLeft).TurnOff();
                office.Socket(Office.SocketFrontLeft).TurnOff();
            });
        }
    }
}
