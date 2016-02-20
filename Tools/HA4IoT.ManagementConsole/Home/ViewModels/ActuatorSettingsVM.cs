namespace HA4IoT.ManagementConsole.Home.ViewModels
{
    public class ActuatorSettingsVM
    {
        public ActuatorSettingsVM()
        {
            AppSettings = new ActuatorAppSettingsVM();
        }

        public bool IsEnabled { get; set; }

        public ActuatorAppSettingsVM AppSettings { get; private set; }
    }
}
