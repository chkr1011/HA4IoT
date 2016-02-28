using System;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class FloatSettingVM : SettingItemVM
    {
        private float _value;
        
        public static FloatSettingVM CreateFrom(JObject source, string key, float defaultValue, string caption)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var value = (float)source.GetNamedNumber(key, (decimal)defaultValue);
            return new FloatSettingVM(key, value, caption);
        }

        public FloatSettingVM(string key, float value, string caption) : base(key, caption)
        {
            _value = value;
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
