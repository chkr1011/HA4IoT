namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class BoolSettingVM : SettingBaseVM
    {
        private bool _value;

        public BoolSettingVM(string caption, bool initialValue) : base(caption)
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
    }
}
