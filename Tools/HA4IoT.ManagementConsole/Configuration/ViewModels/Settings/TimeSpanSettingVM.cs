using System;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class TimeSpanSettingVM : SettingItemVM
    {
        private TimeSpan _value;

        public TimeSpanSettingVM(string key, TimeSpan value, string caption) : base(key, caption)
        {
            _value = value;
        }

        public static TimeSpanSettingVM CreateFrom(JObject source, string key, TimeSpan defaultValue, string caption)
        {
            string value = source.GetNamedString(key, defaultValue.ToString());
            return new TimeSpanSettingVM(key, TimeSpan.Parse(value), caption);
        }

        public TimeSpan Value
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
            return new JValue(Value.ToString());
        }
    }
}
