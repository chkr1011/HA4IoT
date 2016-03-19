using System.Diagnostics;
using System.Windows.Input;

namespace HA4IoT.ManagementConsole.WeatherStation.Views
{
    public partial class WeatherStationTabView
    {
        public WeatherStationTabView()
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
