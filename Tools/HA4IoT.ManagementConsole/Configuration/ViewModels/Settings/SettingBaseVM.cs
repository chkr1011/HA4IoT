using HA4IoT.ManagementConsole.Core;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels.Settings
{
    public abstract class SettingBaseVM : ViewModelBase
    {
        public SettingBaseVM(string key, string caption)
        {
            Key = key;
            Caption = caption;
            IsAppSetting = true;
        }

        public string Key { get; private set; }

        public string Caption { get; private set; }

        public bool IsAppSetting { get; set; }

        public abstract JValue SerializeValue();
    }
}
