using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class FloatSettingVM : SettingBaseVM
    {
        private float _value;

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
