using HA4IoT.ManagementConsole.Chrome.ViewModel;

namespace HA4IoT.ManagementConsole.Chrome.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new MainWindowVM();
            DataContext = viewModel;

            Loaded += (s, e) => viewModel.ConfigurationTab.RefreshAsync();
        }
    }
}
