using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
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
        private readonly StackPanel _demoLampsStackPanel;
        private readonly StackPanel _demoButtonStackPanel;

        public Controller(StackPanel demoLampsStackPanel, StackPanel demoButtonStackPanel)
        {
            if (demoLampsStackPanel == null) throw new ArgumentNullException(nameof(demoLampsStackPanel));
            if (demoButtonStackPanel == null) throw new ArgumentNullException(nameof(demoButtonStackPanel));

            _demoLampsStackPanel = demoLampsStackPanel;
            _demoButtonStackPanel = demoButtonStackPanel;
        }

        protected override void Initialize()
        {
            var area = new Area(new AreaId("TestArea"), this);
            area.AddComponent(new Lamp(new ComponentId("Lamp1"), CreateDemoBinaryComponent("Lamp 1")));
            area.AddComponent(new Lamp(new ComponentId("Lamp2"), CreateDemoBinaryComponent("Lamp 2")));
            area.AddComponent(new Lamp(new ComponentId("Lamp3"), CreateDemoBinaryComponent("Lamp 3")));
            area.AddComponent(new Lamp(new ComponentId("Lamp4"), CreateDemoBinaryComponent("Lamp 4")));
            area.AddComponent(new Lamp(new ComponentId("Lamp5"), CreateDemoBinaryComponent("Lamp 5")));

            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button1"), CreateDemoButton("Button 1"), Timer));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button2"), CreateDemoButton("Button 2"), Timer));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button3"), CreateDemoButton("Button 3"), Timer));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button4"), CreateDemoButton("Button 4"), Timer));
            area.AddComponent(new Sensors.Buttons.Button(new ComponentId("Button5"), CreateDemoButton("Button 5"), Timer));

            area.GetComponent<IButton>(new ComponentId("Button1")).GetPressedShortlyTrigger().Attach(area.GetComponent<ILamp>(new ComponentId("Lamp1")).GetSetNextStateAction());

            AddArea(area);
        }

        private IBinaryStateEndpoint CreateDemoBinaryComponent(string caption)
        {
            CheckBox checkBox = null;

            _demoLampsStackPanel.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    checkBox = new CheckBox();
                    checkBox.IsEnabled = false;
                    checkBox.Content = caption;

                    _demoLampsStackPanel.Children.Add(checkBox);
                }).AsTask().Wait();

            return new CheckBoxBinaryStateEndpoint(checkBox);
        }

        private IButtonEndpoint CreateDemoButton(string caption)
        {
            Button button = null;

            _demoButtonStackPanel.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    button = new Button();
                    button.Content = caption;

                    _demoButtonStackPanel.Children.Add(button);
                }).AsTask().Wait();

            return new UIButtonButtonEndpoint(button);
        }
    }
}
