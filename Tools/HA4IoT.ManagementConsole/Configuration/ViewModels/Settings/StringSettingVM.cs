using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class StringSettingVM : SettingBaseVM
    {
        private string _value;

        public StringSettingVM(string key, string caption, string initialValue) : base(key, caption)
        {
            _value = initialValue;
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
