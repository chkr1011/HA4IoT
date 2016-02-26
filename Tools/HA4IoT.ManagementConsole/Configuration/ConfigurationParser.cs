using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.ManagementConsole.Configuration.ViewModels;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration
{
    public class ConfigurationParser
    {
        public IList<AreaItemVM> Parse(JObject configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var areas = new List<AreaItemVM>();
            foreach (JProperty area in ((JObject)configuration["Areas"]).Properties())
            {
                areas.Add(new AreaParser(area).Parse());
            }

            areas.Sort((x, y) => x.SortValue.CompareTo(y.SortValue));

            return areas;
        }

        public AreaItemVM CreateAreaItemVM(JProperty source)
        {


            var item = new AreaItemVM(source.Name);

            var actuators = ((JObject) source.Value["Actuators"]).Properties().Select(CreateActuatorItemVM).OrderBy(a => a.SortValue);
            item.Actuators.AddRange(actuators);

            var automations = ((JObject) source.Value["Automations"]).Properties().Select(CreateAutomationItemVM);
            item.Automations.AddRange(automations);

            return item;
        }

        public ActuatorItemVM CreateActuatorItemVM(JProperty source)
        {
            return new ActuatorParser(source).Parse();
        }

        public AutomationItemVM CreateAutomationItemVM(JProperty source)
        {
            string type = source.Value["Type"].Value<string>();

            var settings = CreateAutomationSettingsVM((JObject)source.Value["Settings"]);
            var item = new AutomationItemVM(source.Name, type, settings);

            return item;
        }

        public AutomationSettingsVM CreateAutomationSettingsVM(JObject configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var settings = new AutomationSettingsVM();

            settings.IsEnabled = configuration.GetNamedBoolean("IsEnabled");

            return settings;
        }
    }
}
