using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.ManagementConsole.Configuration.ViewModels;
using HA4IoT.ManagementConsole.Configuration.ViewModels.Settings;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration
{
    public class AutomationParser
    {
        private readonly JProperty _source;

        private JObject _settings;
        private JObject _appSettings;

        private string _type;

        public AutomationParser(JProperty source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _source = source;
        }

        public AutomationItemVM Parse()
        {
            _type = _source.Value["Type"].Value<string>();
            _settings = (JObject)_source.Value["Settings"];
            _appSettings = _settings.GetNamedObject("AppSettings", new JObject());

            var item = new AutomationItemVM(_source.Name, _type);
            item.SortValue = (int)_appSettings.GetNamedNumber("SortValue", 0);

            item.Settings.AddRange(GenerateGeneralSettings());

            item.IsEnabled = (BoolSettingVM)item.Settings.First(s => s.Key == "IsEnabled");
            item.Caption = (StringSettingVM)item.Settings.First(s => s.Key == "Caption");

            switch (_type)
            {
                case "HA4IoT.Automations.RollerShutterAutomation":
                    {
                        item.Settings.AddRange(GenerateRollerShutterAutoamtionSettings());
                        break;
                    }
            }

            return item;
        }

        private IEnumerable<SettingItemVM> GenerateRollerShutterAutoamtionSettings()
        {
            yield return BoolSettingVM.CreateFrom(_appSettings, "DoNotOpenBeforeIsEnabled", true, "'Do not open before' enabled").WithIsNoAppSetting();
            yield return TimeSpanSettingVM.CreateFrom(_appSettings, "DoNotOpenBeforeTime", TimeSpan.Parse("07:15:00"), "'Do not open before' time").WithIsNoAppSetting();

            yield return BoolSettingVM.CreateFrom(_appSettings, "DoNotOpenIfTooColdIsEnabled", true, "'Do not open if frozen' enabled").WithIsNoAppSetting();
            yield return FloatSettingVM.CreateFrom(_appSettings, "DoNotOpenIfTooColdTemperature", 2, "'Do not open if frozen' temperature").WithIsNoAppSetting();

            yield return BoolSettingVM.CreateFrom(_appSettings, "AutoCloseIfTooHotIsEnabled", true, "'Close if too hot' enabled").WithIsNoAppSetting();
            yield return FloatSettingVM.CreateFrom(_appSettings, "AutoCloseIfTooHotTemperaure", 25F, "'Close if too hot' temperature").WithIsNoAppSetting();

            yield return
                TimeSpanSettingVM.CreateFrom(_appSettings, "OpenOnSunriseOffset", TimeSpan.Parse("-00:30:00"),
                    "Sunrise offset").WithIsNoAppSetting();

            yield return
                TimeSpanSettingVM.CreateFrom(_appSettings, "CloseOnSunsetOffset", TimeSpan.Parse("00:30:00"),
                    "Sunset offset").WithIsNoAppSetting();
        }

        private IEnumerable<SettingItemVM> GenerateGeneralSettings()
        {
            yield return new StringSettingVM("Id", _source.Name, "ID").WithIsReadOnly();
            yield return new StringSettingVM("Type", _type, "Type").WithIsReadOnly();
            yield return BoolSettingVM.CreateFrom(_settings, "IsEnabled", true, "Enabled").WithIsNoAppSetting();
            yield return StringSettingVM.CreateFrom(_appSettings, "Caption", _source.Name, "Caption");
        }
    }
}
