using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.ManagementConsole.Configuration.ViewModels;
using HA4IoT.ManagementConsole.Configuration.ViewModels.Settings;
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
                areas.Add(CreateAreaItemVM(area));
            }

            return areas;
        }

        public AreaItemVM CreateAreaItemVM(JProperty source)
        {
            var item = new AreaItemVM(source.Name);

            var actuators = ((JObject) source.Value["Actuators"]).Properties().Select(CreateActuatorItemVM).OrderBy(a => a.Settings.AppSettings.SortValue);
            item.Actuators.AddRange(actuators);

            var automations = ((JObject) source.Value["Automations"]).Properties().Select(CreateAutomationItemVM);
            item.Automations.AddRange(automations);

            return item;
        }

        public ActuatorItemVM CreateActuatorItemVM(JProperty source)
        {
            return new ActuatorParser(source).Parse();
        }

        private class ActuatorParser
        {
            private readonly JProperty _source;
            private JObject _appSettings;

            public ActuatorParser(JProperty source)
            {
                if (source == null) throw new ArgumentNullException(nameof(source));

                _source = source;
            }

            public ActuatorItemVM Parse()
            {
                string type = _source.Value["Type"].Value<string>();

                var settings = CreateActuatorSettingsVM((JObject)_source.Value["Settings"]);
                var item = new ActuatorItemVM(_source.Name, type, settings);

                item.ExtendedSettings.Add(new BoolSettingVM("IsEnabled", _appSettings.GetNamedBoolean("IsEnabled", true)));
                item.ExtendedSettings.Add(new StringSettingVM("Image", _appSettings.GetNamedString("Image", "DefaultActuator")));
                item.ExtendedSettings.Add(new StringSettingVM("Caption", _appSettings.GetNamedString("Caption", string.Empty)));
                item.ExtendedSettings.Add(new StringSettingVM("Caption (Overviews)", _appSettings.GetNamedString("OverviewCaption", string.Empty)));
                item.ExtendedSettings.Add(new BoolSettingVM("Hide", _appSettings.GetNamedBoolean("Hide", false)));
                item.ExtendedSettings.Add(new BoolSettingVM("DisplayVertical", _appSettings.GetNamedBoolean("Hide", false)));
                item.ExtendedSettings.Add(new BoolSettingVM("Is part of 'On-State' counter", _appSettings.GetNamedBoolean("IsPartOfOnStateCounter", false)));
                item.ExtendedSettings.Add(new StringSettingVM("'On-State' ID", _appSettings.GetNamedString("OnStateId", "On")));
                item.ExtendedSettings.Add(new IntSettingVM("Max position", (int)_appSettings.GetNamedNumber("MaxPosition", 20000)));
                item.ExtendedSettings.Add(new FloatSettingVM("Max outside temperature for 'Auto Close'", (float)_appSettings.GetNamedNumber("MaxOutsideTemperatureForAutoClose", 26)));
                item.ExtendedSettings.Add(new IntSettingVM("Max moving duration", (int)_appSettings.GetNamedNumber("Max", 26)));

                return item;
            }

            public ActuatorSettingsVM CreateActuatorSettingsVM(JObject configuration)
            {
                if (configuration == null) throw new ArgumentNullException(nameof(configuration));

                var settings = new ActuatorSettingsVM();

                settings.IsEnabled = configuration.GetNamedBoolean("IsEnabled");

                _appSettings = configuration.GetNamedObject("AppSettings", null);
                if (_appSettings != null)
                {
                    settings.AppSettings.Caption = _appSettings.GetNamedString("Caption", string.Empty);
                    settings.AppSettings.OverviewCaption = _appSettings.GetNamedString("OverviewCaption", string.Empty);
                    settings.AppSettings.Image = _appSettings.GetNamedString("Image", string.Empty);
                    settings.AppSettings.SortValue = (int)_appSettings.GetNamedNumber("SortValue", 0.0M);
                    settings.AppSettings.Hide = _appSettings.GetNamedBoolean("Hide", false);
                    settings.AppSettings.IsPartOfOnStateCounter = _appSettings.GetNamedBoolean("IsPartOfOnStateCounter", false);
                    settings.AppSettings.OnState = _appSettings.GetNamedString("OnState", "On");
                    settings.AppSettings.DisplayVertical = _appSettings.GetNamedBoolean("DisplayVertical", false);
                }

                return settings;
            }
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
