using System;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Core;

namespace HA4IoT.Controller.Local
{
    public class Controller : ControllerBase
    {
        private readonly MainPage _mainPage;

        public Controller(MainPage mainPage)
        {
            if (mainPage == null) throw new ArgumentNullException(nameof(mainPage));

            _mainPage = mainPage;
        }

        protected override async void Initialize()
        {
            var area = new Area(new AreaId("TestArea"), this);
            area.AddComponent(new Lamp(new ComponentId("Lamp1"), await _mainPage.CreateDemoBinaryComponent("Lamp 1")));
            area.AddComponent(new Lamp(new ComponentId("Lamp2"), await _mainPage.CreateDemoBinaryComponent("Lamp 2")));
            area.AddComponent(new Lamp(new ComponentId("Lamp3"), await _mainPage.CreateDemoBinaryComponent("Lamp 3")));
            area.AddComponent(new Lamp(new ComponentId("Lamp4"), await _mainPage.CreateDemoBinaryComponent("Lamp 4")));
            area.AddComponent(new Lamp(new ComponentId("Lamp5"), await _mainPage.CreateDemoBinaryComponent("Lamp 5")));

            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button1"), await _mainPage.CreateDemoButton("Button 1"), Timer));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button2"), await _mainPage.CreateDemoButton("Button 2"), Timer));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button3"), await _mainPage.CreateDemoButton("Button 3"), Timer));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button4"), await _mainPage.CreateDemoButton("Button 4"), Timer));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button5"), await _mainPage.CreateDemoButton("Button 5"), Timer));

            area.GetComponent<IButton>(new ComponentId("Button1")).GetPressedShortlyTrigger().Attach(area.GetComponent<ILamp>(new ComponentId("Lamp1")).GetSetNextStateAction());
            area.GetComponent<IButton>(new ComponentId("Button1")).GetPressedLongTrigger().Attach(area.GetComponent<ILamp>(new ComponentId("Lamp2")).GetSetNextStateAction());

            AddArea(area);
        }
    }
}
