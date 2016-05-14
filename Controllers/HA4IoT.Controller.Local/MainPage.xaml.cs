using Windows.UI.Xaml;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Controller.Local
{
    public sealed partial class MainPage
    {
        private readonly Controller _controller;

        public MainPage()
        {
            InitializeComponent();

            _controller = new Controller(DemoLampsStackPanel, DemoButtonStackPanel);

            // The app is only available from other machines. https://msdn.microsoft.com/en-us/library/windows/apps/Hh780593.aspx
            StoragePathTextBox.Text = StoragePath.Root;
            AppPathTextBox.Text = StoragePath.AppRoot;
        }

        private void StartController(object sender, RoutedEventArgs e)
        {
            Log.Instance = new TextBoxLogger(LogTextBox);

            _controller.RunAsync();
        }
    }
}
