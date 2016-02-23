namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class StringSettingVM : SettingBaseVM
    {
        private string _value;

        public StringSettingVM(string caption, string initialValue) : base(caption)
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
    }
}
