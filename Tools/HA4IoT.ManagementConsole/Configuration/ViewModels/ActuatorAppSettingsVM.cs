namespace HA4IoT.ManagementConsole.Configuration.ViewModels
{
    public class ActuatorAppSettingsVM
    {
        public int SortValue { get; set; }

        public string Caption { get; set; }

        public string OverviewCaption { get; set; }

        public string Image { get; set; }

        public bool Hide { get; set; }

        public bool IsPartOfOnStateCounter { get; set; }

        public string OnState { get; set; }

        public bool DisplayVertical { get; set; }
    }
}
