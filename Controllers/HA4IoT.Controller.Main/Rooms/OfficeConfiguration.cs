using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class OfficeConfiguration
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly IDaylightService _daylightService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly SynonymService _synonymService;
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
            SynonymService synonymService,
            RemoteSocketService remoteSocketService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (remoteSocketService == null) throw new ArgumentNullException(nameof(remoteSocketService));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _deviceService = deviceService;
            _areaService = areaService;
            _daylightService = daylightService;
            _ccToolsBoardService = ccToolsBoardService;
            _synonymService = synonymService;
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

            var room = _areaService.CreateArea(Room.Office);

            _sensorFactory.RegisterWindow(room, Office.WindowLeft,
                w => w.WithLeftCasement(input4.GetInput(11)).WithRightCasement(input4.GetInput(12), input4.GetInput(10)));

            _sensorFactory.RegisterWindow(room, Office.WindowRight,
                w => w.WithLeftCasement(input4.GetInput(8)).WithRightCasement(input4.GetInput(9), input5.GetInput(8)));

            _sensorFactory.RegisterTemperatureSensor(room, Office.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(room, Office.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(room, Office.MotionDetector, input4.GetInput(13));

            _actuatorFactory.RegisterSocket(room, Office.SocketFrontLeft, hsrel8.GetOutput(0));
            _actuatorFactory.RegisterSocket(room, Office.SocketFrontRight, hsrel8.GetOutput(6));
            _actuatorFactory.RegisterSocket(room, Office.SocketWindowLeft, hsrel8.GetOutput(10).WithInvertedState());
            _actuatorFactory.RegisterSocket(room, Office.SocketWindowRight, hsrel8.GetOutput(11).WithInvertedState());
            _actuatorFactory.RegisterSocket(room, Office.SocketRearLeftEdge, hsrel8.GetOutput(7));
            _actuatorFactory.RegisterSocket(room, Office.SocketRearLeft, hsrel8.GetOutput(2));
            _actuatorFactory.RegisterSocket(room, Office.SocketRearRight, hsrel8.GetOutput(1));
            _actuatorFactory.RegisterSocket(room, Office.RemoteSocketDesk, _remoteSocketService.GetOutput(0));

            _sensorFactory.RegisterButton(room, Office.ButtonUpperLeft, input5.GetInput(0));
            _sensorFactory.RegisterButton(room, Office.ButtonLowerLeft, input5.GetInput(1));
            _sensorFactory.RegisterButton(room, Office.ButtonLowerRight, input4.GetInput(14));
            _sensorFactory.RegisterButton(room, Office.ButtonUpperRight, input4.GetInput(15));

            _actuatorFactory.RegisterStateMachine(room, Office.CombinedCeilingLights, (s, a) => SetupLight(s, hsrel8, hspe8, a));

            room.GetButton(Office.ButtonUpperLeft).GetPressedLongTrigger().Attach(() =>
            {
                room.GetStateMachine(Office.CombinedCeilingLights).TryTurnOff();
                room.GetSocket(Office.SocketRearLeftEdge).TryTurnOff();
                room.GetSocket(Office.SocketRearLeft).TryTurnOff();
                room.GetSocket(Office.SocketFrontLeft).TryTurnOff();
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

            _synonymService.AddSynonymsForArea(Room.Office, "Büro", "Arbeitszimmer");

            _synonymService.AddSynonymsForComponent(Room.Office, Office.CombinedCeilingLights, "Licht");
            _synonymService.AddSynonymsForComponent(Room.Office, Office.SocketRearLeftEdge, "Rotlicht", "Pufflicht", "Rot");

            _synonymService.AddSynonymsForComponentState(deskOnlyStateId, "Schreibtisch");
            _synonymService.AddSynonymsForComponentState(couchOnlyStateId, "Couch");
        }
    }
}
