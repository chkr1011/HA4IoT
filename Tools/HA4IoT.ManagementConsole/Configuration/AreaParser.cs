using System;
using System.Collections.Generic;
using HA4IoT.ManagementConsole.Configuration.ViewModels;
using HA4IoT.ManagementConsole.Configuration.ViewModels.Settings;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration
{
    public class AreaParser
    {
        private readonly JProperty _source;
        private JObject _appSettings;

        public AreaParser(JProperty source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _source = source;
        }

        public AreaItemVM Parse()
        {
            var settings = (JObject)_source.Value["settings"];
            _appSettings = settings.GetNamedObject("appSettings", null);

            var areaItem = new AreaItemVM(_source.Name);
            areaItem.SortValue = (int)_appSettings.GetNamedNumber("SortValue", 0);
            areaItem.Caption = StringSettingVM.CreateFrom(_appSettings, "Caption", _source.Name, "Caption");
            areaItem.Settings.Add(areaItem.Caption);
            
            areaItem.Actuators.AddRange(ParseActuators());
            areaItem.Automations.AddRange(ParseAutomations());
            
            return areaItem;
        }

        private List<ActuatorItemVM> ParseActuators()
        {
            var actuatorProperties = ((JObject) _source.Value["components"]).Properties();

            var actuators = new List<ActuatorItemVM>();
            foreach (var actuatorProperty in actuatorProperties)
            {
                actuators.Add(new ActuatorParser(actuatorProperty).Parse());
            }

            actuators.Sort((x, y) => x.SortValue.CompareTo(y.SortValue));

            return actuators;
        }

        private List<AutomationItemVM> ParseAutomations()
        {
            var automationProperties = ((JObject)_source.Value["automations"]).Properties();

            var automations = new List<AutomationItemVM>();
            foreach (JProperty automationProperty in automationProperties)
            {
                automations.Add(new AutomationParser(automationProperty).Parse());
            }

            automations.Sort((x, y) => string.Compare(x.Caption.Value, y.Caption.Value, StringComparison.Ordinal));

            return automations;
        } 
    }
}
