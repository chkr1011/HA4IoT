using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Connectors;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core;
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
            // Setup the health monitor which tracks the average time and let an LED blink if everything is healthy.
            InitializeHealthMonitor(LedGpio);

            // Setup the controller which provides ports from the GPIOs of the Pi2.
            var pi2PortController = new Pi2PortController();

            // Setup the wrapper for I2C bus access.
            var i2CBus = new BuiltInI2CBus(Logger);

            // Setup the controller which creates ports for IO boards from CCTools (or based on PCF8574/MAX7311/PCA9555D).
            var ccToolsBoardController = new CCToolsBoardController(this, i2CBus, HttpApiController, Logger);
            var hspe16 = ccToolsBoardController.CreateHSPE16InputOnly(InstalledDevice.HSPE16, new I2CSlaveAddress(41));
            var hsrel8 = ccToolsBoardController.CreateHSREL8(InstalledDevice.HSRel8, new I2CSlaveAddress(40));
            var hsrel5 = ccToolsBoardController.CreateHSREL5(InstalledDevice.HSRel5, new I2CSlaveAddress(56));

            // Setup the remote switch 433Mhz sender which is attached to the I2C bus (Arduino Nano).
            var i2CHardwareBridge = new I2CHardwareBridge(new DeviceId("HB"), new I2CSlaveAddress(50), i2CBus, Timer);
            var remoteSwitchSender = new LPD433MHzSignalSender(i2CHardwareBridge, I2CHardwareBridge433MHzSenderPin, HttpApiController);

            // Setup the controller which creates ports for wireless sockets (433Mhz).
            var ic = new IntertechnoCodeSequenceProvider();
            var remoteSwitchController = new RemoteSocketController(new DeviceId("RemoteSocketController"), remoteSwitchSender, Timer)
                .WithRemoteSocket(0, ic.GetSequence(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1, RemoteSocketCommand.TurnOn), ic.GetSequence(IntertechnoSystemCode.A, IntertechnoUnitCode.Unit1, RemoteSocketCommand.TurnOff));

            // Setup the weather station which provides sunrise and sunset information.
            AddDevice(new OpenWeatherMapWeatherStation(OpenWeatherMapWeatherStation.DefaultDeviceId, Timer, HttpApiController, Logger));

            // Add the example area with the example actuators.
            var area = this.CreateArea(Room.ExampleRoom)
                .WithTemperatureSensor(ExampleRoom.TemperatureSensor, i2CHardwareBridge.DHT22Accessor.GetTemperatureSensor(5))
                .WithHumiditySensor(ExampleRoom.HumiditySensor, i2CHardwareBridge.DHT22Accessor.GetHumiditySensor(5))
                .WithMotionDetector(ExampleRoom.MotionDetector, hspe16.GetInput(8))
                .WithWindow(ExampleRoom.Window, w => w.WithCenterCasement(hspe16.GetInput(0)))
                .WithLamp(ExampleRoom.Lamp1, remoteSwitchController.GetOutput(0))
                .WithSocket(ExampleRoom.Socket1, hsrel5.GetOutput(0))
                .WithSocket(ExampleRoom.Socket2, hsrel5.GetOutput(4))
                .WithSocket(ExampleRoom.BathroomFan, hsrel5.GetOutput(3))
                .WithLamp(ExampleRoom.Lamp2, hsrel8.GetOutput(0))
                .WithLamp(ExampleRoom.Lamp3, hsrel8.GetOutput(1))
                .WithLamp(ExampleRoom.Lamp4, hsrel8.GetOutput(2))
                .WithLamp(ExampleRoom.Lamp5, hsrel8.GetOutput(3))
                .WithLamp(ExampleRoom.Lamp6, hsrel8.GetOutput(4))
                .WithButton(ExampleRoom.Button1, hspe16.GetInput(1))
                .WithButton(ExampleRoom.Button2, hspe16.GetInput(2))
                .WithVirtualButtonGroup(ExampleRoom.LedStripRemote, g => SetupLEDStripRemote(i2CHardwareBridge, g))
                .WithStateMachine(ExampleRoom.CeilingFan, (sm, r) => SetupCeilingFan(sm));
            
            area.Lamp(ExampleRoom.Lamp5).ConnectToggleActionWith(area.Button(ExampleRoom.Button1));
            area.Lamp(ExampleRoom.Lamp6).ConnectToggleActionWith(area.Button(ExampleRoom.Button1), ButtonPressedDuration.Long);
            area.StateMachine(ExampleRoom.CeilingFan).ConnectMoveNextAndToggleOffWith(area.Button(ExampleRoom.Button2));

            SetupHumidityDependingOutput(area.HumiditySensor(ExampleRoom.HumiditySensor), hsrel8.GetOutput(5));

            area.SetupTurnOnAndOffAutomation()
                .WithTrigger(area.MotionDetector(ExampleRoom.MotionDetector))
                .WithTarget(area.BinaryStateOutput(ExampleRoom.BathroomFan))
                .WithTarget(area.BinaryStateOutput(ExampleRoom.Lamp2))
                .WithOnDuration(TimeSpan.FromSeconds(10));

            Timer.Tick += (s, e) =>
            {
                pi2PortController.PollOpenInputPorts();
                ccToolsBoardController.PollInputBoardStates();
            };
        }

        private void SetupHumidityDependingOutput(IHumiditySensor sensor, IBinaryOutput output)
        {
            sensor.ValueChanged += (s, e) =>
            {
                if (e.NewValue > 80.0F)
                {
                    output.Write(BinaryState.High);
                }
                else
                {
                    output.Write(BinaryState.Low);
                }
            };
        }

        private void SetupCeilingFan(StateMachine stateMachine)
        {
            var relayBoard = Device<HSREL5>(DeviceIdFactory.CreateIdFrom(InstalledDevice.HSRel5));
            var gear1 = relayBoard.GetOutput(2);
            var gear2 = relayBoard.GetOutput(1);

            stateMachine.AddOffState().WithLowPort(gear1).WithLowPort(gear2);

            stateMachine.AddState("1").WithHighPort(gear1).WithLowPort(gear2);
            stateMachine.AddState("2").WithLowPort(gear1).WithHighPort(gear2);
        }

        private void SetupLEDStripRemote(I2CHardwareBridge i2CHardwareBridge, VirtualButtonGroup group)
        {
            var ledStripRemote = new LEDStripRemote(i2CHardwareBridge, 4);

            group.WithButton(new ActuatorId("on"), b => b.WithShortAction(() => ledStripRemote.TurnOn()))
                .WithButton(new ActuatorId("off"), b => b.WithShortAction(() => ledStripRemote.TurnOff()))
                .WithButton(new ActuatorId("white"), b => b.WithShortAction(() => ledStripRemote.TurnWhite()))

                .WithButton(new ActuatorId("red1"), b => b.WithShortAction(() => ledStripRemote.TurnRed1()))
                .WithButton(new ActuatorId("green1"), b => b.WithShortAction(() => ledStripRemote.TurnGreen1()))
                .WithButton(new ActuatorId("blue1"), b => b.WithShortAction(() => ledStripRemote.TurnBlue1()));
        }
    }
}
