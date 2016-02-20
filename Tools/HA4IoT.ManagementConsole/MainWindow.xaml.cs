using HA4IoT.ManagementConsole.Home.ViewModels;

namespace HA4IoT.ManagementConsole
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new HomeTabVM();
        }
    }
}
