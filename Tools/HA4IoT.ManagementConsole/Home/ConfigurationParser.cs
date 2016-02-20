using System;
using System.Collections.Generic;
using HA4IoT.ManagementConsole.Home.ViewModels;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Home
{
    public class ConfigurationParser
    {
        public IList<AreaItemVM> Parse(JObject configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var areas = new List<AreaItemVM>();
            foreach (JProperty area in ((JObject)configuration["Areas"]).Properties())
            {
                areas.Add(CreateAreaItemVM(area));
            }

            return areas;
        }

        public AreaItemVM CreateAreaItemVM(JProperty source)
        {
            var item = new AreaItemVM(source.Name);

            foreach (JProperty actuator in ((JObject)source.Value["Actuators"]).Properties())
            {
                item.Actuators.Add(CreateActuatorItemVM(actuator));
            }

            foreach (JProperty automation in ((JObject)source.Value["Automations"]).Properties())
            {
                item.Automations.Add(CreateAutomationItemVM(automation));
            }

            //var value = source.Value<JObject>();
            return item;
        }

        public ActuatorItemVM CreateActuatorItemVM(JProperty source)
        {
            string type = source.Value["Type"].Value<string>();

            var editor = CreateActuatorEditorVM((JObject)source.Value);
            var item = new ActuatorItemVM(source.Name, type, editor);
            
            return item;
        }

        public AutomationItemVM CreateAutomationItemVM(JProperty source)
        {
            //string type = source.Value["Type"].Value<string>();

            var item = new AutomationItemVM(source.Name);

            return item;
        }

        public ActuatorSettingsVM CreateActuatorEditorVM(JObject configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var settings = new ActuatorSettingsVM();

            settings.IsEnabled = configuration.GetNamedBoolean("IsEnabled");

            JObject appSettings = configuration.GetNamedObject("AppSettings", null);
            if (appSettings != null)
            {
                settings.AppSettings.Caption = appSettings.GetNamedString("Caption", string.Empty);
                settings.AppSettings.OverviewCaption = appSettings.GetNamedString("OverviewCaption", string.Empty);
                settings.AppSettings.Image = appSettings.GetNamedString("Image", string.Empty);
                settings.AppSettings.SortValue = (int)appSettings.GetNamedNumber("SortValue", 0.0M);
                settings.AppSettings.Hide = appSettings.GetNamedBoolean("Hide", false);
                settings.AppSettings.IsPartOfOnStateCounter = appSettings.GetNamedBoolean("IsPartOfOnStateCounter", false);
                settings.AppSettings.OnState = appSettings.GetNamedString("OnState", "On");
                settings.AppSettings.DisplayVertical = appSettings.GetNamedBoolean("DisplayVertical", false);
            }

            return settings;
        }
    }
}
