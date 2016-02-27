using System;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class StringSettingVM : SettingBaseVM
    {
        private string _value;

        public StringSettingVM(string key, JObject source, string defaultValue, string caption) : base(key, caption)
        {
            _value = source.GetNamedString(key, defaultValue);
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
