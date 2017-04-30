using System;
using System.Threading.Tasks;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Adapters;
using HA4IoT.Components;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.Outpost;
using HA4IoT.Hardware.Sonoff;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.MotionDetectors;

namespace HA4IoT.Simulator
{
    public class Configuration : IConfiguration
    {
        private readonly MainPage _mainPage;
        private readonly IContainer _containerService;

        public Configuration(MainPage mainPage, IContainer containerService)
        {
            if (mainPage == null) throw new ArgumentNullException(nameof(mainPage));
            if (containerService == null) throw new ArgumentNullException(nameof(containerService));

            _mainPage = mainPage;
            _containerService = containerService;
        }

        public async Task ApplyAsync()
        {
            var areaRepository = _containerService.GetInstance<IAreaRegistryService>();
            var timerService = _containerService.GetInstance<ITimerService>();
            var settingsService = _containerService.GetInstance<ISettingsService>();
            var deviceMessageBroker = _containerService.GetInstance<IDeviceMessageBrokerService>();
            var sonoffDeviceService = _containerService.GetInstance<SonoffDeviceService>();
            var outpostDeviceService = _containerService.GetInstance<OutpostDeviceService>();

            var area = areaRepository.RegisterArea("TestArea");

            area.RegisterComponent(new Lamp("Lamp1", await _mainPage.CreateUIBinaryOutputAdapter("Lamp 1")));
            area.RegisterComponent(new Lamp("Lamp2", await _mainPage.CreateUIBinaryOutputAdapter("Lamp 2")));
            area.RegisterComponent(new Lamp("Lamp3", await _mainPage.CreateUIBinaryOutputAdapter("Lamp 3")));
            area.RegisterComponent(new Lamp("Lamp4", await _mainPage.CreateUIBinaryOutputAdapter("Lamp 4")));
            area.RegisterComponent(new Lamp("Lamp5", await _mainPage.CreateUIBinaryOutputAdapter("Lamp 5")));

            area.RegisterComponent(new Lamp("RGBS1", outpostDeviceService.GetRgbStripAdapter("RGBS1")));

            area.RegisterComponent(new Socket("Socket1", await _mainPage.CreateUIBinaryOutputAdapter("Socket 1")));
            area.RegisterComponent(new Socket("Socket2", await _mainPage.CreateUIBinaryOutputAdapter("Socket 2")));
            area.RegisterComponent(new Socket("Socket3", await _mainPage.CreateUIBinaryOutputAdapter("Socket 3")));
            area.RegisterComponent(new Socket("Socket4", await _mainPage.CreateUIBinaryOutputAdapter("Socket 4")));
            area.RegisterComponent(new Socket("Socket5", await _mainPage.CreateUIBinaryOutputAdapter("Socket 5")));

            area.RegisterComponent(new Socket("Socket_POW_01", sonoffDeviceService.GetAdapterForPow("SonoffPow_01")));

            area.RegisterComponent(new Button("Button1", await _mainPage.CreateUIButtonAdapter("Button 1"), timerService, settingsService));
            area.RegisterComponent(new Button("Button2", await _mainPage.CreateUIButtonAdapter("Button 2"), timerService, settingsService));
            area.RegisterComponent(new Button("Button3", await _mainPage.CreateUIButtonAdapter("Button 3"), timerService, settingsService));
            area.RegisterComponent(new Button("Button4", await _mainPage.CreateUIButtonAdapter("Button 4"), timerService, settingsService));
            area.RegisterComponent(new Button("Button5_SONOFF", new VirtualButtonAdapter(), timerService, settingsService));

            area.RegisterComponent(new MotionDetector("Motion1", await _mainPage.CreateUIMotionDetectorAdapter("Motion Detector 1"), _containerService.GetInstance<ISchedulerService>(), _containerService.GetInstance<ISettingsService>()));
            
            area.GetComponent<IButton>("Button1").PressedLongTrigger.Attach(() => area.GetComponent<ILamp>("Lamp2").TryTogglePowerState());

            area.GetComponent<IButton>("Button2").PressedShortTrigger.Attach(() =>
            {
                area.GetComponent<ILamp>("RGBS1").TryTogglePowerState();
            });

            area.GetComponent<IButton>("Button3")
                .PressedShortTrigger
                .Attach(() => area.GetComponent<ISocket>("Socket1").TryTogglePowerState());

            area.GetComponent<IButton>("Button4")
                .PressedShortTrigger
                .Attach(() => area.GetComponent<ISocket>("Socket2").TryTogglePowerState());

            area.GetComponent<IButton>("Button5_SONOFF")
                .PressedShortTrigger
                .Attach(() => area.GetComponent<ISocket>("Socket_POW_01").TryTogglePowerState());
        }
    }
}
