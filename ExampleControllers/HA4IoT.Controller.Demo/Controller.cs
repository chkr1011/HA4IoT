using System;
using System.Threading.Tasks;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Automations;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Core;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.ExternalServices.TelegramBot;
using HA4IoT.ExternalServices.Twitter;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Codes;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.HumiditySensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Sensors.TemperatureSensors;
using HA4IoT.Sensors.Windows;

namespace HA4IoT.Controller.Demo
{
    internal class Controller : ControllerBase
    {
        private const int LedGpio = 22;
        private const byte I2CHardwareBridge433MHzSenderPin = 6;
        
        protected override void Initialize()
        {
            InitializeHealthMonitor(LedGpio);

            AddDevice(new BuiltInI2CBus());

            var piPortController = new Pi2PortController();
            AddDevice(piPortController);

            var ccToolsBoardController = new CCToolsBoardController(this, GetDevice<II2CBus>());
            AddDevice(ccToolsBoardController);

            // Setup the remote switch 433Mhz sender which is attached to the I2C bus (Arduino Nano).
            AddDevice(new I2CHardwareBridge(new I2CSlaveAddress(50), GetDevice<II2CBus>(), Timer));

            RegisterService(new OpenWeatherMapWeatherService(Timer, ApiController));
            
            SetupRoom();

            Timer.Tick += (s, e) =>
            {
                piPortController.PollOpenInputPorts();
                ccToolsBoardController.PollInputBoardStates();
            };

            SetupDemo();
        }

        private void SetupDemo()
        {
            // Get the area from the controller.
            IArea area = this.GetArea(Room.ExampleRoom);

            // Get the single motion detector from the controller.
            IMotionDetector motionDetector = GetComponent<IMotionDetector>();
            ITrigger motionDetectedTrigger = motionDetector.GetMotionDetectedTrigger();

            // Get the single temperature and humidity sensor from the controller.
            ITemperatureSensor temperatureSensor = GetComponent<ITemperatureSensor>();
            IHumiditySensor humiditySensor = GetComponent<IHumiditySensor>();

            // Get the button with the specified ID from the area (not globally).
            IButton button = area.GetButton(ExampleRoom.Button1);
            ITrigger buttonTrigger = button.GetPressedShortlyTrigger();

            // Get a test lamp from the area (not globally).
            ILamp lamp2 = area.GetLamp(ExampleRoom.Lamp2);
            ILamp lamp3 = area.GetLamp(ExampleRoom.Lamp3);
            
            // Integrate the twitter client if the configuration file is available.
            TwitterClient twitterClient;
            if (TwitterClientFactory.TryCreateFromDefaultConfigurationFile(out twitterClient))
            {
                RegisterService(new TwitterClient());
                
                IAction tweetAction = twitterClient.GetTweetAction($"Someone is here ({DateTime.Now})... @chkratky");

                motionDetectedTrigger.Attach(tweetAction);
                buttonTrigger.Attach(tweetAction);
            }

            // An automation is "Fulfilled" per default.
            var automation = new Automation(new AutomationId("DemoAutomation"))
                .WithTrigger(motionDetectedTrigger)
                .WithActionIfConditionsFulfilled(lamp3.GetTurnOnAction())
                .WithCondition(ConditionRelation.And, new ComponentIsInStateCondition(lamp2, BinaryStateId.Off))
                .WithCondition(ConditionRelation.And, new NumericValueSensorHasValueGreaterThanCondition(humiditySensor, 80));

            AddAutomation(automation);

            TelegramBot telegramBot;
            if (!TelegramBotFactory.TryCreateFromDefaultConfigurationFile(out telegramBot))
            {
                RegisterService(telegramBot);

                Task.Run(async () => await telegramBot.TrySendMessageToAdministratorsAsync("Das Demo-System wurde neu gestartet."));
                telegramBot.MessageReceived += HandleTelegramBotMessage;
            }
        }

        private async void HandleTelegramBotMessage(object sender, TelegramBotMessageReceivedEventArgs e)
        {
            await Task.FromResult(0);

            ////if (e.Message.GetIsPatternMatch("Hi"))
            ////{
            ////    await e.SendResponse("Was geht?");
            ////}
            ////else if (e.Message.GetIsPatternMatch("auf.*Toilette"))
            ////{
            ////    var motionDetector = GetComponent<IMotionDetector>(new ComponentId("ExampleRoom.MotionDetector"));
            ////    if (motionDetector.GetState().Equals(MotionDetectorStateId.MotionDetected))
            ////    {
            ////        await e.SendResponse("Die Toilette ist gerade besetzt.");
            ////    }
            ////    else
            ////    {
            ////        await e.SendResponse("Die Toilette ist frei!");
            ////    }
            ////}
            ////else if (e.Message.GetIsPatternMatch("Licht.*an"))
            ////
            ////    var light = GetComponent<IActuator>(new ComponentId("ExampleRoom.Lamp1"));
            ////    light.SetState(BinaryStateId.On);

            ////    await e.SendResponse("Ich habe das Licht eingeschaltet.");
            ////}
            ////else if (e.Message.GetIsPatternMatch("Licht.*aus"))
            ////{
            ////    var light = GetComponent<IActuator>(new ComponentId("ExampleRoom.Lamp1"));
            ////    light.SetState(BinaryStateId.Off);

            ////    await e.SendResponse("Ich habe das Licht ausgeschaltet.");
            ////}
            ////else
            ////{
            ////    await e.SendResponse("Was willst du von mir?");
            ////}
        }

        private void SetupRoom()
        {
            var ccToolsBoardController = GetDevice<CCToolsBoardController>();

            var hspe16 = ccToolsBoardController.CreateHSPE16InputOnly(InstalledDevice.HSPE16, new I2CSlaveAddress(41));
            var hsrel8 = ccToolsBoardController.CreateHSREL8(InstalledDevice.HSRel8, new I2CSlaveAddress(40));
            var hsrel5 = ccToolsBoardController.CreateHSREL5(InstalledDevice.HSRel5, new I2CSlaveAddress(56));

            var i2CHardwareBridge = GetDevice<I2CHardwareBridge>();
            var remoteSwitchSender = new LPD433MHzSignalSender(i2CHardwareBridge, I2CHardwareBridge433MHzSenderPin, ApiController);

            var intertechno = new IntertechnoCodeSequenceProvider();
            var remoteSwitchController = new RemoteSocketController(remoteSwitchSender, Timer)
                .WithRemoteSocket(0, intertechno.GetSequencePair(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1))
                .WithRemoteSocket(1, intertechno.GetSequencePair(IntertechnoSystemCode.B, IntertechnoUnitCode.Unit1));

            const int SensorPin = 5;

            var area = this.CreateArea(Room.ExampleRoom)
                .WithTemperatureSensor(ExampleRoom.TemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(SensorPin))
                .WithHumiditySensor(ExampleRoom.HumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(SensorPin))
                .WithMotionDetector(ExampleRoom.MotionDetector, hspe16[HSPE16Pin.GPIO8])

                .WithLamp(ExampleRoom.Lamp1, remoteSwitchController.GetOutput(0))
                .WithLamp(ExampleRoom.Lamp2, remoteSwitchController.GetOutput(1))

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

                .WithStateMachine(ExampleRoom.CeilingFan, (sm, r) => SetupCeilingFan(sm))
                .WithWindow(ExampleRoom.Window, w => w.WithCenterCasement(hspe16[HSPE16Pin.GPIO0]));

            area.GetButton(ExampleRoom.Button1).GetPressedShortlyTrigger().Attach(area.GetLamp(ExampleRoom.Lamp5).GetSetNextStateAction());
            area.GetButton(ExampleRoom.Button1).ConnectToggleActionWith(area.GetLamp(ExampleRoom.Lamp6), ButtonPressedDuration.Long);

            area.GetStateMachine(ExampleRoom.CeilingFan).ConnectMoveNextAndToggleOffWith(area.GetButton(ExampleRoom.Button2));

            SetupHumidityDependingLamp(area.GetHumiditySensor(ExampleRoom.HumiditySensor), area.GetLamp(ExampleRoom.Lamp7));

            area.SetupTurnOnAndOffAutomation()
                .WithTrigger(area.GetMotionDetector(ExampleRoom.MotionDetector))
                .WithTarget(area.GetStateMachine(ExampleRoom.BathroomFan))
                .WithTarget(area.GetLamp(ExampleRoom.Lamp2))
                .WithOnDuration(TimeSpan.FromSeconds(10));

            SetupLEDStripRemote(i2CHardwareBridge, area);
        }

        private void SetupHumidityDependingLamp(IHumiditySensor sensor, ILamp lamp)
        {
            ITrigger trigger = sensor.GetHumidityReachedTrigger(80);
            IAction action = lamp.GetTurnOnAction();

            trigger.Attach(action);
            
            var twitterClient = new TwitterClient();
            trigger.Attach(twitterClient.GetTweetAction("Hello World"));
        }

        private void SetupCeilingFan(StateMachine stateMachine)
        {
            var relayBoard = GetDevice<HSREL5>(DeviceIdFactory.CreateIdFrom(InstalledDevice.HSRel5));
            var gear1 = relayBoard.GetOutput(2);
            var gear2 = relayBoard.GetOutput(1);

            stateMachine.AddOffState().WithLowOutput(gear1).WithLowOutput(gear2);

            stateMachine.AddState(new StatefulComponentState("1")).WithHighOutput(gear1).WithLowOutput(gear2);
            stateMachine.AddState(new StatefulComponentState("2")).WithLowOutput(gear1).WithHighOutput(gear2);
        }

        private void SetupLEDStripRemote(I2CHardwareBridge i2CHardwareBridge, IArea area)
        {
            const int SenderPin = 4;

            var ledStripRemote = new LEDStripRemote(i2CHardwareBridge, SenderPin);

            area.WithVirtualButton(ExampleRoom.ButtonStripOn, b => b.WithPressedShortlyAction(() => ledStripRemote.TurnOn()))
                .WithVirtualButton(ExampleRoom.ButtonStripOff, b => b.WithPressedShortlyAction(() => ledStripRemote.TurnOff()))
                .WithVirtualButton(ExampleRoom.ButtonStripWhite, b => b.WithPressedShortlyAction(() => ledStripRemote.TurnWhite()))

                .WithVirtualButton(ExampleRoom.ButtonStripRed, b => b.WithPressedShortlyAction(() => ledStripRemote.TurnRed1()))
                .WithVirtualButton(ExampleRoom.ButtonStripGreen, b => b.WithPressedShortlyAction(() => ledStripRemote.TurnGreen1()))
                .WithVirtualButton(ExampleRoom.ButtonStripBlue, b => b.WithPressedShortlyAction(() => ledStripRemote.TurnBlue1()));
        }
    }
}
