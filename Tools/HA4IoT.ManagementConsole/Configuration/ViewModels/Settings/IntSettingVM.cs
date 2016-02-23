namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public class IntSettingVM : SettingBaseVM
    {
        private int _value;

        public IntSettingVM(string caption, int initialValue) : base(caption)
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
    }
}
