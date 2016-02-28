using System;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class StringSettingVM : SettingItemVM
    {
        private string _value;

        public StringSettingVM(string key, string value, string caption) : base(key, caption)
        {
            _value = value;
        }

        public static StringSettingVM CreateFrom(JObject source, string key, string defaultValue, string caption)
        {
            string value = source.GetNamedString(key, defaultValue);
            return new StringSettingVM(key, value, caption);
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChangedFromCaller();
            }
        }

        public override JValue SerializeValue()
        {
            return new JValue(Value);
        }
    }
}
