using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class FloatSettingVM : SettingItemVM
    {
        private float _value;
        
        public FloatSettingVM(string key, JObject source, float initialValue, string caption) : base(key, caption)
        {
            _value = (float)source.GetNamedNumber(key, (decimal)initialValue);
        }

        public FloatSettingVM(string key, string caption, float initialValue) : base(key, caption)
        {
            _value = initialValue;
        }

        public float Value
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
