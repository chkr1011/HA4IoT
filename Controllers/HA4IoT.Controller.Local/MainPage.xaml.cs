using System.IO;
using Windows.UI.Xaml;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Controller.Local
{
    public sealed partial class MainPage
    {
        private readonly Controller _controller = new Controller();

        public MainPage()
        {
            InitializeComponent();
        }

        private void StartController(object sender, RoutedEventArgs e)
        {
            Log.Instance = new TextBoxLogger(LogTextBox);
            StoragePath.AppRoot = new DirectoryInfo(@".\..\..\..\..\..\..\App\HA4IoT.WebApp").FullName;

            _controller.RunAsync();
        }
    }
}
