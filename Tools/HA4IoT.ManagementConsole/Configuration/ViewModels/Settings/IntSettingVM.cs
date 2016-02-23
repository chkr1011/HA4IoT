using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class IntSettingVM : SettingBaseVM
    {
        private int _value;

        public IntSettingVM(string key, string caption, int initialValue) : base(key, caption)
        {
            _value = initialValue;
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
