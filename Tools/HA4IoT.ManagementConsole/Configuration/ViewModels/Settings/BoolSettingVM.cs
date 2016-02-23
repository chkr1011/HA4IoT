using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class BoolSettingVM : SettingBaseVM
    {
        private bool _value;

        public BoolSettingVM(string key, string caption, bool initialValue) : base(key, caption)
        {
            _value = initialValue;
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
