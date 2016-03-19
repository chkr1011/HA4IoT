using System.Diagnostics;
using System.Windows.Input;

namespace HA4IoT.ManagementConsole.WeatherStationConfiguration.Views
{
    public partial class WeatherStationConfigurationView
    {
        public WeatherStationConfigurationView()
        {
            InitializeComponent();
        }

        private void OpenSignUpPage(object sender, MouseButtonEventArgs e)
        {
            using (Process.Start("https://home.openweathermap.org/users/sign_up"))
            {
            }
        }
    }
}
