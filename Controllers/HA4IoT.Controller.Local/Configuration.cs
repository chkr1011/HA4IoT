using System;
using System.Threading.Tasks;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

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

            var area = areaRepository.RegisterArea(new AreaId("TestArea"));

            area.AddComponent(new Lamp(new ComponentId("Lamp1"), await _mainPage.CreateDemoBinaryComponent("Lamp 1")));
            area.AddComponent(new Lamp(new ComponentId("Lamp2"), await _mainPage.CreateDemoBinaryComponent("Lamp 2")));
            area.AddComponent(new Lamp(new ComponentId("Lamp3"), await _mainPage.CreateDemoBinaryComponent("Lamp 3")));
            area.AddComponent(new Lamp(new ComponentId("Lamp4"), await _mainPage.CreateDemoBinaryComponent("Lamp 4")));
            area.AddComponent(new Lamp(new ComponentId("Lamp5"), await _mainPage.CreateDemoBinaryComponent("Lamp 5")));

            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button1"), await _mainPage.CreateDemoButton("Button 1"), timerService, settingsService));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button2"), await _mainPage.CreateDemoButton("Button 2"), timerService, settingsService));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button3"), await _mainPage.CreateDemoButton("Button 3"), timerService, settingsService));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button4"), await _mainPage.CreateDemoButton("Button 4"), timerService, settingsService));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button5"), await _mainPage.CreateDemoButton("Button 5"), timerService, settingsService));

            area.GetComponent<IButton>(new ComponentId("Button1")).PressedShortlyTrigger.Attach(area.GetComponent<ILamp>(new ComponentId("Lamp1")).GetSetNextStateAction());
            area.GetComponent<IButton>(new ComponentId("Button1")).PressedLongTrigger.Attach(area.GetComponent<ILamp>(new ComponentId("Lamp2")).GetSetNextStateAction());

            area.GetComponent<IButton>("Button3".AsComponentId())
                .PressedShortlyTrigger
                .Attach(area.GetComponent<ISocket>("Socket1".AsComponentId()).GetSetNextStateAction());

            area.GetComponent<IButton>("Button4".AsComponentId())
                .PressedShortlyTrigger
                .Attach(area.GetComponent<ISocket>("Socket2".AsComponentId()).GetSetNextStateAction());

            area.GetComponent<IButton>("Button5".AsComponentId())
                .PressedShortlyTrigger
                .Attach(area.GetComponent<ISocket>("Socket3".AsComponentId()).GetSetNextStateAction());
        }
    }
}
