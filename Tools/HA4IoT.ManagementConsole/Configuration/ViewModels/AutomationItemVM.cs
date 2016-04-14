using System;
using HA4IoT.ManagementConsole.Configuration.ViewModels.Settings;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels
{
    public class AutomationItemVM : ConfigurationItemVM
    {
        public AutomationItemVM(string id, string type) : base(id)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            
            Type = type;
        }

        public string Type { get; set; }

        public int SortValue { get; set; }

        public BoolSettingVM IsEnabled { get; set; }

        public StringSettingVM Caption { get; set; }
        
        public JObject SerializeSettings()
        {
            var configuration = new JObject();
            var appSettings = new JObject();
            configuration["appSettings"] = appSettings;

            appSettings["SortValue"] = new JValue(SortValue);

            foreach (var setting in Settings)
            {
                if (setting.IsReadOnly)
                {
                    continue;
                }

                if (setting.IsAppSetting)
                {
                    appSettings[setting.Key] = setting.SerializeValue();
                }
                else
                {
                    configuration[setting.Key] = setting.SerializeValue();
                }
            }

            return configuration;
        }
    }
}
