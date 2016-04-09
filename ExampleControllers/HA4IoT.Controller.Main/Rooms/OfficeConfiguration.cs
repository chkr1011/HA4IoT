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

            WindowLeft,
            WindowRight
        }

        public OfficeConfiguration(IController controller) 
            : base(controller)
        {
        }

        public override void Setup()
        {
            var hsrel8 = CCToolsBoardController.CreateHSREL8(InstalledDevice.OfficeHSREL8, new I2CSlaveAddress(20));
            var hspe8 = CCToolsBoardController.CreateHSPE8OutputOnly(InstalledDevice.UpperFloorAndOfficeHSPE8, new I2CSlaveAddress(37));
            var input4 = Controller.Device<HSPE16InputOnly>(InstalledDevice.Input4);
            var input5 = Controller.Device<HSPE16InputOnly>(InstalledDevice.Input5);

            const int SensorPin = 2;

            var i2cHardwareBridge = Controller.GetDevice<I2CHardwareBridge>();

            var room = Controller.CreateArea(Room.Office)
                .WithMotionDetector(Office.MotionDetector, input4.GetInput(13))
                .WithTemperatureSensor(Office.TemperatureSensor, i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(Office.HumiditySensor, i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
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
                .WithStateMachine(Office.CombinedCeilingLights, (s, a) => SetupLight(s, hsrel8, hspe8, a));
            
            room.GetButton(Office.ButtonUpperLeft).GetPressedLongTrigger().Attach(() =>
            {
                room.GetStateMachine(Office.CombinedCeilingLights).TryTurnOff();
                room.Socket(Office.SocketRearLeftEdge).TryTurnOff();
                room.Socket(Office.SocketRearLeft).TryTurnOff();
                room.Socket(Office.SocketFrontLeft).TryTurnOff();
            });
        }

        private void SetupLight(StateMachine light, HSREL8 hsrel8, HSPE8OutputOnly hspe8, IArea room)
        {
            // Front lights (left, middle, right)
            var fl = hspe8[HSPE8Pin.GPIO0].WithInvertedState();
            var fm = hspe8[HSPE8Pin.GPIO2].WithInvertedState();
            var fr = hsrel8[HSREL8Pin.GPIO0].WithInvertedState();

            // Middle lights (left, middle, right)
            var ml = hspe8[HSPE8Pin.GPIO1].WithInvertedState();
            var mm = hspe8[HSPE8Pin.GPIO3].WithInvertedState();
            var mr = hsrel8[HSREL8Pin.GPIO1].WithInvertedState();
            
            // Rear lights (left, right)
            var rl = hsrel8[HSREL8Pin.GPIO5].WithInvertedState();
            var rr = hsrel8[HSREL8Pin.GPIO4].WithInvertedState();

            light.AddOffState()
                .WithLowOutput(fl)
                .WithLowOutput(fm)
                .WithLowOutput(fr)
                .WithLowOutput(ml)
                .WithLowOutput(mm)
                .WithLowOutput(mr)
                .WithLowOutput(rl)
                .WithLowOutput(rr);

            light.AddOnState()
                .WithHighOutput(fl)
                .WithHighOutput(fm)
                .WithHighOutput(fr)
                .WithHighOutput(ml)
                .WithHighOutput(mm)
                .WithHighOutput(mr)
                .WithHighOutput(rl)
                .WithHighOutput(rr);

            var deskOnlyStateId = new StatefulComponentState("DeskOnly");
            light.AddState(deskOnlyStateId)
                .WithHighOutput(fl)
                .WithHighOutput(fm)
                .WithLowOutput(fr)
                .WithHighOutput(ml)
                .WithLowOutput(mm)
                .WithLowOutput(mr)
                .WithLowOutput(rl)
                .WithLowOutput(rr);

            var couchOnlyStateId = new StatefulComponentState("CouchOnly");
            light.AddState(couchOnlyStateId)
                .WithLowOutput(fl)
                .WithLowOutput(fm)
                .WithLowOutput(fr)
                .WithLowOutput(ml)
                .WithLowOutput(mm)
                .WithLowOutput(mr)
                .WithLowOutput(rl)
                .WithHighOutput(rr);

            light.WithTurnOffIfStateIsAppliedTwice();

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
