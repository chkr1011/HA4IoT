using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Core;

namespace HA4IoT.Controller.Local
{
    public sealed partial class MainPage
    {
        private readonly Core.Controller _controller;

        public MainPage()
        {
            InitializeComponent();

            Log.Instance = new TextBoxLogger(LogTextBox);

            var options = new ControllerOptions
            {
                ConfigurationType = typeof(Initializer),
                ContainerConfigurator = new ContainerConfigurator(this),
                HttpServerPort = 1025
            };

            _controller = new Core.Controller(options);
            
            // The app is only available from other machines. https://msdn.microsoft.com/en-us/library/windows/apps/Hh780593.aspx
            StoragePathTextBox.Text = StoragePath.Root;
            AppPathTextBox.Text = StoragePath.AppRoot;
        }

        private class ContainerConfigurator : IContainerConfigurator
        {
            private readonly MainPage _mainPage;

            public ContainerConfigurator(MainPage mainPage)
            {
                if (mainPage == null) throw new ArgumentNullException(nameof(mainPage));

                _mainPage = mainPage;
            }

            public void ConfigureContainer(IContainer containerService)
            {
                if (containerService == null) throw new ArgumentNullException(nameof(containerService));

                containerService.RegisterSingleton(() => _mainPage);
            }
        }

        public async Task<IBinaryStateEndpoint> CreateDemoBinaryComponent(string caption)
        {
            IBinaryStateEndpoint result = null;

            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    var checkBox = new CheckBox();
                    checkBox.IsEnabled = false;
                    checkBox.Content = caption;

                    var endpoint = new CheckBoxBinaryStateEndpoint(checkBox);

                    DemoLampsStackPanel.Children.Add(checkBox);

                    result = endpoint;
                });

            return result;
        }

        public async Task<IButtonEndpoint> CreateDemoButton(string caption)
        {
            IButtonEndpoint result = null;

            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    var button = new Button();
                    button.Content = caption;

                    var endpoint = new UIButtonButtonEndpoint(button);
                    endpoint.Connect();

                    DemoButtonStackPanel.Children.Add(button);

                    result = endpoint;
                });

            return result;
        }

        private void StartController(object sender, RoutedEventArgs e)
        {
            StartControllerButton.IsEnabled = false;

            _controller.RunAsync();
        }

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = string.Empty;
        }
    }
}
