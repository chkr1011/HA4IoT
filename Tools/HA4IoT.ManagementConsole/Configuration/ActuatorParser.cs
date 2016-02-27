using System;
using HA4IoT.ManagementConsole.Configuration.ViewModels;
using HA4IoT.ManagementConsole.Configuration.ViewModels.Settings;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration
{
    public class ActuatorParser
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

            var settings = (JObject)_source.Value["Settings"];
            _appSettings = settings.GetNamedObject("AppSettings", null);
            
            var item = new ActuatorItemVM(_source.Name, type);
            item.SortValue = (int)_appSettings.GetNamedNumber("SortValue", 0);

            var isEnabledSetting = new BoolSettingVM("IsEnabled", settings, true, "Enabled") { IsAppSetting = false };
            item.Settings.Add(isEnabledSetting);
            item.IsEnabled = isEnabledSetting;

            var imageSetting = new StringSettingVM("Image", _appSettings, "DefaultActuator", "Image");
            item.Settings.Add(imageSetting);
            item.Image = imageSetting;

            var captionSetting = new StringSettingVM("Caption", _appSettings, _source.Name, "Caption");
            item.Settings.Add(captionSetting);
            item.Caption = captionSetting;

            item.Settings.Add(new StringSettingVM("OverviewCaption", _appSettings, _source.Name, "Caption (Overviews)"));
            item.Settings.Add(new BoolSettingVM("Hide", _appSettings, false, "Hidden"));
            item.Settings.Add(new BoolSettingVM("DisplayVertical", _appSettings, false, "Display vertical"));
            item.Settings.Add(new BoolSettingVM("IsPartOfOnStateCounter", _appSettings, false, "Is part of 'On-State' counter"));
            item.Settings.Add(new StringSettingVM("OnStateId", _appSettings, "On", "'On-State' ID"));

            item.Settings.Add(new IntSettingVM("MaxPosition", _appSettings, 20000, "Max position"));
            //item.ExtendedSettings.Add(new FloatSettingVM("Max outside temperature for 'Auto Close'", (float)_appSettings.GetNamedNumber("MaxOutsideTemperatureForAutoClose", 26)));
            //item.Settings.Add(new IntSettingVM("MaxMovingDuration" "Max moving duration", (int)_appSettings.GetNamedNumber("MaxMovingDuration", 26)));

            return item;
        }
    }
}
