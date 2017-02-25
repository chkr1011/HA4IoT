using System;
using System.Threading.Tasks;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Components;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.Sonoff;
using HA4IoT.Sensors.Buttons;

namespace HA4IoT.Controller.Local
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
            var sonoffDeviceService = _containerService.GetInstance<SonoffDeviceService>();

            var area = areaRepository.RegisterArea("TestArea");

            area.AddComponent(new Lamp("Lamp1", await _mainPage.CreateDemoBinaryComponent("Lamp 1")));
            area.AddComponent(new Lamp("Lamp2", await _mainPage.CreateDemoBinaryComponent("Lamp 2")));
            area.AddComponent(new Lamp("Lamp3", await _mainPage.CreateDemoBinaryComponent("Lamp 3")));
            area.AddComponent(new Lamp("Lamp4", await _mainPage.CreateDemoBinaryComponent("Lamp 4")));
            area.AddComponent(new Lamp("Lamp5", await _mainPage.CreateDemoBinaryComponent("Lamp 5")));

            area.AddComponent(new Socket("Socket1", await _mainPage.CreateDemoBinaryComponent("Socket 1")));
            area.AddComponent(new Socket("Socket2", await _mainPage.CreateDemoBinaryComponent("Socket 2")));
            area.AddComponent(new Socket("Socket3", await _mainPage.CreateDemoBinaryComponent("Socket 3")));
            area.AddComponent(new Socket("Socket4", await _mainPage.CreateDemoBinaryComponent("Socket 4")));
            area.AddComponent(new Socket("Socket5", await _mainPage.CreateDemoBinaryComponent("Socket 5")));

            area.AddComponent(new Socket("Socket_POW_01", sonoffDeviceService.GetBinaryOutputAdapter("SonoffPow_01")));

            area.AddComponent(new Button("Button1", await _mainPage.CreateDemoButton("Button 1"), timerService, settingsService));
            area.AddComponent(new Button("Button2", await _mainPage.CreateDemoButton("Button 2"), timerService, settingsService));
            area.AddComponent(new Button("Button3", await _mainPage.CreateDemoButton("Button 3"), timerService, settingsService));
            area.AddComponent(new Button("Button4", await _mainPage.CreateDemoButton("Button 4"), timerService, settingsService));
            area.AddComponent(new Button("Button5", await _mainPage.CreateDemoButton("Button 5"), timerService, settingsService));

            area.GetComponent<IButton>("Button1").PressedShortlyTrigger.Attach(() => area.GetComponent<ILamp>("Lamp1").TryTogglePowerState());
            area.GetComponent<IButton>("Button1").PressedLongTrigger.Attach(() => area.GetComponent<ILamp>("Lamp2").TryTogglePowerState());

            area.GetComponent<IButton>("Button3")
                .PressedShortlyTrigger
                .Attach(() => area.GetComponent<ISocket>("Socket1").TryTogglePowerState());

            area.GetComponent<IButton>("Button4")
                .PressedShortlyTrigger
                .Attach(() => area.GetComponent<ISocket>("Socket2").TryTogglePowerState());

            area.GetComponent<IButton>("Button5")
                .PressedShortlyTrigger
                .Attach(() => area.GetComponent<ISocket>("Socket_POW_01").TryTogglePowerState());
        }
    }
}
