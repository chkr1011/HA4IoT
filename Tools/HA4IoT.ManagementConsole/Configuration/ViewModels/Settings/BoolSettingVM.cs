using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class BoolSettingVM : SettingItemVM
    {
        private bool _value;

        public BoolSettingVM(string key, bool value, string caption) : base(key, caption)
        {
            _value = value;
        }

        public static BoolSettingVM CreateFrom(JObject source, string key, bool defaultValue, string caption)
        {
            bool value = source.GetNamedBoolean(key, defaultValue);
            return new BoolSettingVM(key, value, caption);
        }

        public bool Value
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
