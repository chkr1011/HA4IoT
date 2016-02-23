using HA4IoT.ManagementConsole.Core;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public abstract class SettingBaseVM : ViewModelBase
    {
        public SettingBaseVM(string caption)
        {
            Caption = caption;
        }

        public string Caption { get; private set; }
    }
}
