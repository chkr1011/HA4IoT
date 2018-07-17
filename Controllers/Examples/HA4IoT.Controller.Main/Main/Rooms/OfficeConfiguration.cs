using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Areas;
using HA4IoT.Components;
using HA4IoT.Components.Adapters.MqttBased;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.RemoteSockets;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Hardware.Drivers.CCTools.Devices;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal class OfficeConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly IRemoteSocketService _remoteSocketService;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly IMessageBrokerService _messageBroker;
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;
        private readonly ILogService _logService;

        private enum Office
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
            ButtonEdgeLeft,
            ButtonEdgeRight,

            RgbLight,
            CombinedCeilingLights,

            WindowLeftL,
            WindowLeftR,

            WindowRightL,
            WindowRightR
        }

        public OfficeConfiguration(
            IDeviceRegistryService deviceService,
            IAreaRegistryService areaService,
            IRemoteSocketService remoteSocketService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            IMessageBrokerService messageBroker,
            IDeviceMessageBrokerService deviceMessageBroker,
            ILogService logService)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _deviceMessageBroker = deviceMessageBroker ?? throw new ArgumentNullException(nameof(deviceMessageBroker));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _remoteSocketService = remoteSocketService ?? throw new ArgumentNullException(nameof(remoteSocketService));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
        }

        public void Apply()
        {
            var input4 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input4.ToString());
            var input5 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input5.ToString());

            var hsrel8 = _deviceService.GetDevice<HSREL8>(InstalledDevice.OfficeHSREL8.ToString());
            var hspe8 = _deviceService.GetDevice<HSPE8OutputOnly>(InstalledDevice.UpperFloorAndOfficeHSPE8.ToString());

            //var hsrel8 = (HSREL8)_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSRel8, InstalledDevice.OfficeHSREL8.ToString(), 20);
            //var hspe8 = (HSPE8OutputOnly)_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSPE8_OutputOnly, InstalledDevice.UpperFloorAndOfficeHSPE8.ToString(), 37);


            var area = _areaService.RegisterArea(Room.Office);

            //var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();
            //const int SensorPin = 6; // ID from MQTT
            //_sensorFactory.RegisterTemperatureSensor(area, Office.TemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));
            //_sensorFactory.RegisterHumiditySensor(area, Office.HumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterWindow(area, Office.WindowLeftL, input4.GetInput(11));
            _sensorFactory.RegisterWindow(area, Office.WindowLeftR, input4.GetInput(12), input4.GetInput(10));
            _sensorFactory.RegisterWindow(area, Office.WindowRightL, input4.GetInput(8));
            _sensorFactory.RegisterWindow(area, Office.WindowRightR, input4.GetInput(9), input5.GetInput(8));

            _sensorFactory.RegisterMotionDetector(area, Office.MotionDetector, input4.GetInput(13));

            _sensorFactory.RegisterTemperatureSensor(area, Office.TemperatureSensor, new MqttBasedNumericSensorAdapter("sensors-office/temperature", _deviceMessageBroker, _logService));
            _sensorFactory.RegisterHumiditySensor(area, Office.HumiditySensor, new MqttBasedNumericSensorAdapter("sensors-office/humidity", _deviceMessageBroker, _logService));

            _actuatorFactory.RegisterLamp(area, Office.RgbLight, new RgbDeviceAdapter("kitchen-rgb/$patch/rgb", _deviceMessageBroker));

            _actuatorFactory.RegisterSocket(area, Office.SocketFrontLeft, hsrel8.GetOutput(0));
            _actuatorFactory.RegisterSocket(area, Office.SocketFrontRight, hsrel8.GetOutput(6));
            _actuatorFactory.RegisterSocket(area, Office.SocketWindowLeft, hsrel8.GetOutput(10).WithInvertedState());
            _actuatorFactory.RegisterSocket(area, Office.SocketWindowRight, hsrel8.GetOutput(11).WithInvertedState());
            _actuatorFactory.RegisterSocket(area, Office.SocketRearLeftEdge, hsrel8.GetOutput(7));
            _actuatorFactory.RegisterSocket(area, Office.SocketRearLeft, hsrel8.GetOutput(2));
            _actuatorFactory.RegisterSocket(area, Office.SocketRearRight, hsrel8.GetOutput(1));
            _actuatorFactory.RegisterSocket(area, Office.RemoteSocketDesk, _remoteSocketService.GetRemoteSocket("OFFICE_0"));

            _sensorFactory.RegisterButton(area, Office.ButtonUpperLeft, input5.GetInput(0));
            _sensorFactory.RegisterButton(area, Office.ButtonUpperRight, input4.GetInput(15));
            _sensorFactory.RegisterButton(area, Office.ButtonLowerLeft, input5.GetInput(1));
            _sensorFactory.RegisterButton(area, Office.ButtonLowerRight, input4.GetInput(14));

            _sensorFactory.RegisterButton(area, Office.ButtonEdgeLeft, new DeviceBinaryInput("office/edge/button-left", _deviceMessageBroker));
            _sensorFactory.RegisterButton(area, Office.ButtonEdgeRight, new DeviceBinaryInput("office/edge/button-right", _deviceMessageBroker));

            var stateMachine = _actuatorFactory.RegisterStateMachine(area, Office.CombinedCeilingLights, (s, a) => SetupLight(s, hsrel8, hspe8));
            stateMachine.AlternativeStateId = StateMachineStateExtensions.OffStateId;
            stateMachine.ResetStateId = StateMachineStateExtensions.OffStateId;

            area.GetButton(Office.ButtonUpperLeft)
                .CreatePressedShortTrigger(_messageBroker)
                .Attach(() => area.GetComponent(Office.CombinedCeilingLights).TrySetState(StateMachineStateExtensions.OnStateId));

            area.GetButton(Office.ButtonUpperLeft).CreatePressedLongTrigger(_messageBroker).Attach(() =>
            {
                area.GetComponent(Office.CombinedCeilingLights).TryTurnOff();
                area.GetComponent(Office.SocketRearLeftEdge).TryTurnOff();
                area.GetComponent(Office.SocketRearLeft).TryTurnOff();
                area.GetComponent(Office.SocketFrontLeft).TryTurnOff();
            });

            area.GetButton(Office.ButtonLowerLeft)
                .CreatePressedShortTrigger(_messageBroker)
                .Attach(() => area.GetComponent(Office.CombinedCeilingLights).TrySetState("DeskOnly"));

            area.GetButton(Office.ButtonLowerRight)
                .CreatePressedShortTrigger(_messageBroker)
                .Attach(() => area.GetComponent(Office.CombinedCeilingLights).TrySetState("CouchOnly"));

            area.GetButton(Office.ButtonEdgeRight)
                .CreatePressedShortTrigger(_messageBroker)
                .Attach(() => area.GetComponent(Office.CombinedCeilingLights).TrySetState(StateMachineStateExtensions.OnStateId));

            area.GetButton(Office.ButtonEdgeLeft)
                .CreatePressedShortTrigger(_messageBroker)
                .Attach(() => area.GetComponent(Office.SocketRearRight).TryTogglePowerState());
        }

        private static void SetupLight(StateMachine light, HSREL8 hsrel8, HSPE8OutputOnly hspe8)
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
                .WithLowBinaryOutput(fl)
                .WithLowBinaryOutput(fm)
                .WithLowBinaryOutput(fr)
                .WithLowBinaryOutput(ml)
                .WithLowBinaryOutput(mm)
                .WithLowBinaryOutput(mr)
                .WithLowBinaryOutput(rl)
                .WithLowBinaryOutput(rr);

            light.AddOnState()
                .WithHighBinaryOutput(fl)
                .WithHighBinaryOutput(fm)
                .WithHighBinaryOutput(fr)
                .WithHighBinaryOutput(ml)
                .WithHighBinaryOutput(mm)
                .WithHighBinaryOutput(mr)
                .WithHighBinaryOutput(rl)
                .WithHighBinaryOutput(rr);

            light.AddState("DeskOnly")
                .WithHighBinaryOutput(fl)
                .WithHighBinaryOutput(fm)
                .WithLowBinaryOutput(fr)
                .WithHighBinaryOutput(ml)
                .WithLowBinaryOutput(mm)
                .WithLowBinaryOutput(mr)
                .WithLowBinaryOutput(rl)
                .WithLowBinaryOutput(rr);

            light.AddState("CouchOnly")
                .WithLowBinaryOutput(fl)
                .WithLowBinaryOutput(fm)
                .WithLowBinaryOutput(fr)
                .WithLowBinaryOutput(ml)
                .WithLowBinaryOutput(mm)
                .WithLowBinaryOutput(mr)
                .WithLowBinaryOutput(rl)
                .WithHighBinaryOutput(rr);

            light.AlternativeStateId = StateMachineStateExtensions.OffStateId;
            light.ResetStateId = StateMachineStateExtensions.OffStateId;
        }
    }
}
