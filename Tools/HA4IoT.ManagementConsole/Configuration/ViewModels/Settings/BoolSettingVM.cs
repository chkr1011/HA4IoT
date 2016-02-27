using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class BoolSettingVM : SettingBaseVM
    {
        private bool _value;

        public BoolSettingVM(string key, JObject source, bool initialValue, string caption) : base(key, caption)
        {
            _value = source.GetNamedBoolean(key, initialValue);
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
