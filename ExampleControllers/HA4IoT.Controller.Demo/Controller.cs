using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Core;
using HA4IoT.ExternalServices.Twitter;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.OpenWeatherMapWeatherStation;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Controller.Demo
{
    internal class Controller : ControllerBase
    {
        private const int LedGpio = 22;
        private const byte I2CHardwareBridge433MHzSenderPin = 6;
        
        protected override void Initialize()
        {
            // Setup the health monitor which tracks the average tick time and let an LED blink if everything is healthy.
            InitializeHealthMonitor(LedGpio);

            // Setup the wrapper for I2C bus access.
            AddDevice(new BuiltInI2CBus());

            // Setup the controller which provides ports from the GPIOs of the Pi2.
            var pi2PortController = new Pi2PortController();
            
            // Setup the controller which creates ports for IO boards from CCTools (or based on PCF8574/MAX7311/PCA9555D).
            var ccToolsBoardController = new CCToolsBoardController(this, GetDevice<II2CBus>(), ApiController);

            SetupRoom(ccToolsBoardController);

            Timer.Tick += (s, e) =>
            {
                pi2PortController.PollOpenInputPorts();
                ccToolsBoardController.PollInputBoardStates();
            };
        }

        private void SetupRoom(CCToolsBoardController ccToolsBoardController)
        {
            var hspe16 = ccToolsBoardController.CreateHSPE16InputOnly(InstalledDevice.HSPE16, new I2CSlaveAddress(41));
            var hsrel8 = ccToolsBoardController.CreateHSREL8(InstalledDevice.HSRel8, new I2CSlaveAddress(40));
            var hsrel5 = ccToolsBoardController.CreateHSREL5(InstalledDevice.HSRel5, new I2CSlaveAddress(56));

            // Setup the remote switch 433Mhz sender which is attached to the I2C bus (Arduino Nano).
            var i2CHardwareBridge = new I2CHardwareBridge(new DeviceId("HB"), new I2CSlaveAddress(50), GetDevice<II2CBus>(), Timer);
            var remoteSwitchSender = new LPD433MHzSignalSender(i2CHardwareBridge, I2CHardwareBridge433MHzSenderPin, ApiController);

            var intertechno = new IntertechnoCodeSequenceProvider();
            var remoteSwitchController = new RemoteSocketController(new DeviceId("RemoteSocketController"), remoteSwitchSender, Timer)
                .WithRemoteSocket(0, intertechno.GetSequencePair(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1))
                .WithRemoteSocket(1, intertechno.GetSequencePair(IntertechnoSystemCode.B, IntertechnoUnitCode.Unit1));

            AddDevice(new OpenWeatherMapWeatherStation(OpenWeatherMapWeatherStation.DefaultDeviceId, Timer, ApiController));

            const int SensorPin = 5;

            var area = this.CreateArea(Room.ExampleRoom)
                .WithTemperatureSensor(ExampleRoom.TemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(ExampleRoom.HumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithMotionDetector(ExampleRoom.MotionDetector, hspe16[HSPE16Pin.GPIO8])

                .WithLamp(ExampleRoom.Lamp1, remoteSwitchController.GetOutput(0))
                .WithLamp(ExampleRoom.Lamp2, remoteSwitchController.GetOutput(0))

                .WithSocket(ExampleRoom.Socket1, hsrel5[HSREL5Pin.Relay0])
                .WithSocket(ExampleRoom.Socket2, hsrel5[HSREL5Pin.Relay4])
                .WithSocket(ExampleRoom.BathroomFan, hsrel5[HSREL5Pin.Relay3])
                .WithLamp(ExampleRoom.Lamp3, hsrel8[HSREL8Pin.Relay0])
                .WithLamp(ExampleRoom.Lamp4, hsrel8[HSREL8Pin.Relay1])
                .WithLamp(ExampleRoom.Lamp5, hsrel8[HSREL8Pin.Relay2])
                .WithLamp(ExampleRoom.Lamp6, hsrel8[HSREL8Pin.Relay3])
                .WithLamp(ExampleRoom.Lamp7, hsrel8[HSREL8Pin.Relay4])
                .WithLamp(ExampleRoom.Lamp8, hsrel8[HSREL8Pin.Relay5])

                .WithButton(ExampleRoom.Button1, hspe16[HSPE16Pin.GPIO1])
                .WithButton(ExampleRoom.Button2, hspe16[HSPE16Pin.GPIO2])

                .WithVirtualButtonGroup(ExampleRoom.LedStripRemote, g => SetupLEDStripRemote(i2CHardwareBridge, g))
                .WithStateMachine(ExampleRoom.CeilingFan, (sm, r) => SetupCeilingFan(sm))
                .WithWindow(ExampleRoom.Window, w => w.WithCenterCasement(hspe16[HSPE16Pin.GPIO0]));

            area.GetButton(ExampleRoom.Button1).GetPressedShortlyTrigger().OnTriggered(area.GetLamp(ExampleRoom.Lamp5).GetToggleStateAction());
            area.GetButton(ExampleRoom.Button1).ConnectToggleActionWith(area.GetLamp(ExampleRoom.Lamp6), ButtonPressedDuration.Long);

            area.GetStateMachine(ExampleRoom.CeilingFan).ConnectMoveNextAndToggleOffWith(area.GetButton(ExampleRoom.Button2));

            SetupHumidityDependingLamp(area.GetHumiditySensor(ExampleRoom.HumiditySensor), area.GetLamp(ExampleRoom.Lamp7));

            area.SetupTurnOnAndOffAutomation()
                .WithTrigger(area.GetMotionDetector(ExampleRoom.MotionDetector))
                .WithTarget(area.BinaryStateOutput(ExampleRoom.BathroomFan))
                .WithTarget(area.BinaryStateOutput(ExampleRoom.Lamp2))
                .WithOnDuration(TimeSpan.FromSeconds(10));
        }

        private void SetupHumidityDependingLamp(IHumiditySensor sensor, ILamp lamp)
        {
            ITrigger trigger = sensor.GetHumidityReachedTrigger(80);
            IHomeAutomationAction action = lamp.GetTurnOnAction();

            trigger.OnTriggered(action);
            
            var twitterClient = new TwitterClient();
            trigger.OnTriggered(twitterClient.GetTweetAction("Hello World"));
        }

        private void SetupCeilingFan(StateMachine stateMachine)
        {
            var relayBoard = GetDevice<HSREL5>(DeviceIdFactory.CreateIdFrom(InstalledDevice.HSRel5));
            var gear1 = relayBoard.GetOutput(2);
            var gear2 = relayBoard.GetOutput(1);

            stateMachine.AddOffState().WithLowPort(gear1).WithLowPort(gear2);

            stateMachine.AddState("1").WithHighPort(gear1).WithLowPort(gear2);
            stateMachine.AddState("2").WithLowPort(gear1).WithHighPort(gear2);
        }

        private void SetupLEDStripRemote(I2CHardwareBridge i2CHardwareBridge, VirtualButtonGroup group)
        {
            const int SenderPin = 4;

            var ledStripRemote = new LEDStripRemote(i2CHardwareBridge, SenderPin);

            group.WithButton(new ActuatorId("on"), b => b.WithPressedShortlyAction(() => ledStripRemote.TurnOn()))
                .WithButton(new ActuatorId("off"), b => b.WithPressedShortlyAction(() => ledStripRemote.TurnOff()))
                .WithButton(new ActuatorId("white"), b => b.WithPressedShortlyAction(() => ledStripRemote.TurnWhite()))

                .WithButton(new ActuatorId("red1"), b => b.WithPressedShortlyAction(() => ledStripRemote.TurnRed1()))
                .WithButton(new ActuatorId("green1"), b => b.WithPressedShortlyAction(() => ledStripRemote.TurnGreen1()))
                .WithButton(new ActuatorId("blue1"), b => b.WithPressedShortlyAction(() => ledStripRemote.TurnBlue1()));
        }
    }
}
