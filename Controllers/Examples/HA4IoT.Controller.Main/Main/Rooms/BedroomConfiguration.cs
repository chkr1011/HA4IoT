using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Adapters;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal class BedroomConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly AutomationFactory _automationFactory;

        private enum Bedroom
        {
            TemperatureSensor,
            HumiditySensor,
            MotionDetector,

            LightCeiling,
            LightCeilingAutomation,
            LightCeilingWindow,
            LightCeilingWall,

            LampBedLeft,
            LampBedRight,

            SocketWindowLeft,
            SocketWindowRight,
            SocketWall,
            SocketWallEdge,
            SocketBedLeft,
            SocketBedRight,

            ButtonDoor,
            ButtonWindowUpper,
            ButtonWindowLower,

            ButtonBedLeftInner,
            ButtonBedLeftOuter,
            ButtonBedRightInner,
            ButtonBedRightOuter,

            RollerShutterButtonsUpperUp,
            RollerShutterButtonsUpperDown,
            RollerShutterButtonsLowerUp,
            RollerShutterButtonsLowerDown,

            RollerShutterLeft,
            RollerShutterRight,
            RollerShuttersAutomation,

            Fan,

            CombinedCeilingLights,

            WindowLeft,
            WindowRight
        }

        public BedroomConfiguration(
            IDeviceRegistryService deviceService,
            IAreaRegistryService areaService,
            CCToolsDeviceService ccToolsBoardService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            AutomationFactory automationFactory)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
        }

        public void Apply()
        {
            var hsrel5 = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.BedroomHSREL5.ToString(), new I2CSlaveAddress(38));
            var hsrel8 = _ccToolsBoardService.RegisterHSREL8(InstalledDevice.BedroomHSREL8.ToString(), new I2CSlaveAddress(21));
            var input5 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input5.ToString());
            var input4 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input4.ToString());
            var i2CHardwareBridge = _deviceService.GetDevice<I2CHardwareBridge>();

            const int SensorPin = 6;

            var area = _areaService.RegisterArea(Room.Bedroom);

            _sensorFactory.RegisterWindow(area, Bedroom.WindowLeft, new PortBasedWindowAdapter(input5.GetInput(2)));
            _sensorFactory.RegisterWindow(area, Bedroom.WindowRight, new PortBasedWindowAdapter(input5.GetInput(3)));

            _sensorFactory.RegisterTemperatureSensor(area, Bedroom.TemperatureSensor,
                i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin));

            _sensorFactory.RegisterHumiditySensor(area, Bedroom.HumiditySensor,
                i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin));

            _sensorFactory.RegisterMotionDetector(area, Bedroom.MotionDetector, input5.GetInput(12));

            _sensorFactory.RegisterButton(area, Bedroom.ButtonWindowUpper, input5.GetInput(10));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonWindowLower, input5.GetInput(13));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonBedLeftInner, input4.GetInput(2));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonBedLeftOuter, input4.GetInput(0));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonBedRightInner, input4.GetInput(1));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonBedRightOuter, input4.GetInput(3));
            _sensorFactory.RegisterButton(area, Bedroom.ButtonDoor, input5.GetInput(11));

            _sensorFactory.RegisterRollerShutterButtons(area, Bedroom.RollerShutterButtonsUpperUp, input5.GetInput(6),
                Bedroom.RollerShutterButtonsUpperDown, input5.GetInput(7));

            _sensorFactory.RegisterRollerShutterButtons(area, Bedroom.RollerShutterButtonsLowerUp, input5.GetInput(4),
                Bedroom.RollerShutterButtonsLowerDown, input5.GetInput(5));

            _actuatorFactory.RegisterLamp(area, Bedroom.LightCeiling, hsrel5.GetOutput(5).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Bedroom.LightCeilingWindow, hsrel5.GetOutput(6).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Bedroom.LightCeilingWall, hsrel5.GetOutput(7).WithInvertedState());
            _actuatorFactory.RegisterLamp(area, Bedroom.LampBedLeft, hsrel5.GetOutput(4));
            _actuatorFactory.RegisterLamp(area, Bedroom.LampBedRight, hsrel8.GetOutput(8).WithInvertedState());

            _actuatorFactory.RegisterSocket(area, Bedroom.SocketWindowLeft, hsrel5[HSREL5Pin.Relay0]);
            _actuatorFactory.RegisterSocket(area, Bedroom.SocketWindowRight, hsrel5[HSREL5Pin.Relay1]);
            _actuatorFactory.RegisterSocket(area, Bedroom.SocketWall, hsrel5[HSREL5Pin.Relay2]);
            _actuatorFactory.RegisterSocket(area, Bedroom.SocketWallEdge, hsrel5[HSREL5Pin.Relay3]);
            _actuatorFactory.RegisterSocket(area, Bedroom.SocketBedLeft, hsrel8.GetOutput(7));
            _actuatorFactory.RegisterSocket(area, Bedroom.SocketBedRight, hsrel8.GetOutput(9));

            _actuatorFactory.RegisterRollerShutter(area, Bedroom.RollerShutterLeft, hsrel8[HSREL8Pin.Relay6], hsrel8[HSREL8Pin.Relay5]);
            _actuatorFactory.RegisterRollerShutter(area, Bedroom.RollerShutterRight, hsrel8[HSREL8Pin.Relay3], hsrel8[HSREL8Pin.Relay4]);

            area.GetRollerShutter(Bedroom.RollerShutterLeft)
                .ConnectWith(area.GetButton(Bedroom.RollerShutterButtonsUpperUp), area.GetButton(Bedroom.RollerShutterButtonsUpperDown));

            area.GetRollerShutter(Bedroom.RollerShutterRight)
                .ConnectWith(area.GetButton(Bedroom.RollerShutterButtonsLowerUp), area.GetButton(Bedroom.RollerShutterButtonsLowerDown));

            var ceilingLights = _actuatorFactory.RegisterLogicalComponent(area, Bedroom.CombinedCeilingLights)
                .WithComponent(area.GetLamp(Bedroom.LightCeilingWall))
                .WithComponent(area.GetLamp(Bedroom.LightCeilingWindow));

            area.GetButton(Bedroom.ButtonDoor).PressedShortTrigger.Attach(() => ceilingLights.TryTogglePowerState());
            area.GetButton(Bedroom.ButtonWindowUpper).PressedShortTrigger.Attach(() => ceilingLights.TryTogglePowerState());

            area.GetButton(Bedroom.ButtonDoor).PressedLongTrigger.Attach(() =>
            {
                area.GetComponent(Bedroom.LampBedLeft).TryTurnOff();
                area.GetComponent(Bedroom.LampBedRight).TryTurnOff();
                area.GetComponent(Bedroom.CombinedCeilingLights).TryTurnOff();
            });

            _automationFactory.RegisterRollerShutterAutomation(area, Bedroom.RollerShuttersAutomation)
                .WithRollerShutters(area.GetComponents<IRollerShutter>())
                .WithDoNotOpenBefore(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))
                .WithCloseIfOutsideTemperatureIsGreaterThan(24)
                .WithDoNotOpenIfOutsideTemperatureIsBelowThan(3);

            _automationFactory.RegisterTurnOnAndOffAutomation(area, Bedroom.LightCeilingAutomation)
                .WithTrigger(area.GetMotionDetector(Bedroom.MotionDetector))
                .WithTarget(area.GetComponent(Bedroom.LightCeiling))
                .WithTurnOnIfAllRollerShuttersClosed(area.GetRollerShutter(Bedroom.RollerShutterLeft), area.GetRollerShutter(Bedroom.RollerShutterRight))
                .WithEnabledAtNight()
                .WithSkipIfAnyIsAlreadyOn(area.GetLamp(Bedroom.LampBedLeft), area.GetLamp(Bedroom.LampBedRight));

            _actuatorFactory.RegisterFan(area, Bedroom.Fan, new BedroomFanAdapter(hsrel8));
            
            area.GetButton(Bedroom.ButtonBedLeftInner).PressedShortTrigger.Attach(() => area.GetComponent(Bedroom.LampBedLeft).TryTogglePowerState());
            area.GetButton(Bedroom.ButtonBedLeftInner).PressedLongTrigger.Attach(() => area.GetComponent(Bedroom.CombinedCeilingLights).TryTogglePowerState());
            area.GetButton(Bedroom.ButtonBedLeftOuter).PressedShortTrigger.Attach(() => area.GetComponent(Bedroom.Fan).TryIncreaseLevel());
            area.GetButton(Bedroom.ButtonBedLeftOuter).PressedLongTrigger.Attach(() => area.GetComponent(Bedroom.Fan).TryTurnOff());

            area.GetButton(Bedroom.ButtonBedRightInner).PressedShortTrigger.Attach(() => area.GetComponent(Bedroom.LampBedRight).TryTogglePowerState());
            area.GetButton(Bedroom.ButtonBedRightInner).PressedLongTrigger.Attach(() => area.GetComponent(Bedroom.CombinedCeilingLights).TryTogglePowerState());
            area.GetButton(Bedroom.ButtonBedRightOuter).PressedShortTrigger.Attach(() => area.GetComponent(Bedroom.Fan).TryIncreaseLevel());
            area.GetButton(Bedroom.ButtonBedRightOuter).PressedLongTrigger.Attach(() => area.GetComponent(Bedroom.Fan).TryTurnOff());
        }

        private class BedroomFanAdapter : IFanAdapter
        {
            private readonly IBinaryOutput _relay0;
            private readonly IBinaryOutput _relay1;
            private readonly IBinaryOutput _relay2;

            public int MaxLevel { get; } = 3;

            public BedroomFanAdapter(HSREL8 hsrel8)
            {
                _relay0 = hsrel8[HSREL8Pin.Relay0];
                _relay1 = hsrel8[HSREL8Pin.Relay1];
                _relay2 = hsrel8[HSREL8Pin.Relay2];
            }

            public void SetState(int level, params IHardwareParameter[] parameters)
            {
                switch (level)
                {
                    case 0:
                        {
                            _relay0.Write(BinaryState.Low);
                            _relay1.Write(BinaryState.Low);
                            _relay2.Write(BinaryState.Low);
                            break;
                        }

                    case 1:
                        {
                            _relay0.Write(BinaryState.High);
                            _relay1.Write(BinaryState.Low);
                            _relay2.Write(BinaryState.High);
                            break;
                        }

                    case 2:
                        {
                            _relay0.Write(BinaryState.High);
                            _relay1.Write(BinaryState.High);
                            _relay2.Write(BinaryState.Low);
                            break;
                        }

                    case 3:
                        {
                            _relay0.Write(BinaryState.High);
                            _relay1.Write(BinaryState.High);
                            _relay2.Write(BinaryState.High);
                            break;
                        }

                    default:
                        {
                            throw new NotSupportedException();
                        }
                }
            }
        }
    }
}
