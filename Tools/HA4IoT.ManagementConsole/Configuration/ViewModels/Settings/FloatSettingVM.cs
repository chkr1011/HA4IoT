namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class FloatSettingVM : SettingBaseVM
    {
        private float _value;

        public FloatSettingVM(string caption, float initialValue) : base(caption)
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
    }
}
