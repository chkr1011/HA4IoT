using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class IntSettingVM : SettingItemVM
    {
        private int _value;

        public IntSettingVM(string key, int value, string caption) : base(key, caption)
        {
            _value = value;
        }

        public static IntSettingVM CreateFrom(JObject source, string key, int defaultValue, string caption)
        {
            int value = (int)source.GetNamedNumber(key, defaultValue);
            return new IntSettingVM(key, value, caption);
        }

        public int Value
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
