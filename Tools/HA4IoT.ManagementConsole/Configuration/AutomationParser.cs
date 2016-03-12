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
            yield return BoolSettingVM.CreateFrom(_appSettings, "SkipBeforeTimestampIsEnabled", true, "Skip before timestamp").WithIsNoAppSetting();
            yield return TimeSpanSettingVM.CreateFrom(_appSettings, "SkipBeforeTimestamp", TimeSpan.Parse("07:15:00"), "Skip before timestamp time").WithIsNoAppSetting();

            yield return BoolSettingVM.CreateFrom(_appSettings, "SkipIfFrozenIsEnabled", true, "Skip if frozen").WithIsNoAppSetting();
            yield return FloatSettingVM.CreateFrom(_appSettings, "SkipIfFrozenTemperature", 2, "Skip if frozen temperature").WithIsNoAppSetting();

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
