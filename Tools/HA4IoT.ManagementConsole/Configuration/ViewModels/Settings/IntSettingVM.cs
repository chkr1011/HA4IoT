using System;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class IntSettingVM : SettingBaseVM
    {
        private int _value;

        public IntSettingVM(string key,  JObject source, int initalValue, string caption) : base(key, caption)
        {
            _value = (int)source.GetNamedNumber(key, initalValue);
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
