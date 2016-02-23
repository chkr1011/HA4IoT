using HA4IoT.ManagementConsole.Configuration.ViewModels.Settings;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels
{
    public class ActuatorAppSettingsVM
    {
        public int SortValue { get; set; }

        public StringSettingVM Caption { get; set; }

        public StringSettingVM OverviewCaption { get; set; }

        public StringSettingVM Image { get; set; }

        public BoolSettingVM Hide { get; set; }
    }
}
