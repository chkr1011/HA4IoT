using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class OfficeConfiguration
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly IDaylightService _daylightService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly RemoteSocketService _remoteSocketService;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;

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

        public OfficeConfiguration(
            IDeviceService deviceService,
            IAreaService areaService,
            IDaylightService daylightService,
            CCToolsBoardService ccToolsBoardService,
            RemoteSocketService remoteSocketService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (remoteSocketService == null) throw new ArgumentNullException(nameof(remoteSocketService));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _deviceService = deviceService;
            _areaService = areaService;
            _daylightService = daylightService;
            _ccToolsBoardService = ccToolsBoardService;
            _remoteSocketService = remoteSocketService;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hsrel8 = _ccToolsBoardService.RegisterHSREL8(InstalledDevice.OfficeHSREL8, new I2CSlaveAddress(20));
            var hspe8 = _ccToolsBoardService.RegisterHSPE8OutputOnly(InstalledDevice.UpperFloorAndOfficeHSPE8, new I2CSlaveAddress(37));
            var input4 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input4);
            var input5 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input5);
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 2;

            var area = _areaService.CreateArea(Room.Office);

            _sensorFactory.RegisterWindow(area, Office.WindowLeft,
                w => w.WithLeftCasement(input4.GetInput(11)).WithRightCasement(input4.GetInput(12), input4.GetInput(10)));

            _sensorFactory.RegisterWindow(area, Office.WindowRight,
                w => w.WithLeftCasement(input4.GetInput(8)).WithRightCasement(input4.GetInput(9), input5.GetInput(8)));

            _sensorFactory.RegisterTemperatureSensor(area, Office.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, Office.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(area, Office.MotionDetector, input4.GetInput(13));

            _actuatorFactory.RegisterSocket(area, Office.SocketFrontLeft, hsrel8.GetOutput(0));
            _actuatorFactory.RegisterSocket(area, Office.SocketFrontRight, hsrel8.GetOutput(6));
            _actuatorFactory.RegisterSocket(area, Office.SocketWindowLeft, hsrel8.GetOutput(10).WithInvertedState());
            _actuatorFactory.RegisterSocket(area, Office.SocketWindowRight, hsrel8.GetOutput(11).WithInvertedState());
            _actuatorFactory.RegisterSocket(area, Office.SocketRearLeftEdge, hsrel8.GetOutput(7));
            _actuatorFactory.RegisterSocket(area, Office.SocketRearLeft, hsrel8.GetOutput(2));
            _actuatorFactory.RegisterSocket(area, Office.SocketRearRight, hsrel8.GetOutput(1));
            _actuatorFactory.RegisterSocket(area, Office.RemoteSocketDesk, _remoteSocketService.GetOutput(0));

            _sensorFactory.RegisterButton(area, Office.ButtonUpperLeft, input5.GetInput(0));
            _sensorFactory.RegisterButton(area, Office.ButtonLowerLeft, input5.GetInput(1));
            _sensorFactory.RegisterButton(area, Office.ButtonLowerRight, input4.GetInput(14));
            _sensorFactory.RegisterButton(area, Office.ButtonUpperRight, input4.GetInput(15));

            _actuatorFactory.RegisterStateMachine(area, Office.CombinedCeilingLights, (s, a) => SetupLight(s, hsrel8, hspe8, a));

            area.GetButton(Office.ButtonUpperLeft).GetPressedLongTrigger().Attach(() =>
            {
                area.GetStateMachine(Office.CombinedCeilingLights).TryTurnOff();
                area.GetSocket(Office.SocketRearLeftEdge).TryTurnOff();
                area.GetSocket(Office.SocketRearLeft).TryTurnOff();
                area.GetSocket(Office.SocketFrontLeft).TryTurnOff();
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
            // Two mechanical relays.
            var rl = hsrel8[HSREL8Pin.GPIO5];
            var rr = hsrel8[HSREL8Pin.GPIO4];

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

            var deskOnlyStateId = new ComponentState("DeskOnly");
            light.AddState(deskOnlyStateId)
                .WithHighOutput(fl)
                .WithHighOutput(fm)
                .WithLowOutput(fr)
                .WithHighOutput(ml)
                .WithLowOutput(mm)
                .WithLowOutput(mr)
                .WithLowOutput(rl)
                .WithLowOutput(rr);

            var couchOnlyStateId = new ComponentState("CouchOnly");
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
                .Attach(light.GetSetStateAction(couchOnlyStateId));

            room.GetButton(Office.ButtonLowerLeft)
                .GetPressedShortlyTrigger()
                .Attach(light.GetSetStateAction(deskOnlyStateId));

            room.GetButton(Office.ButtonUpperLeft)
                .GetPressedShortlyTrigger()
                .Attach(light.GetSetStateAction(BinaryStateId.On));
        }
    }
}
