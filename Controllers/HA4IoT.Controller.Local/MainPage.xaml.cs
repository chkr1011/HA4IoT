using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Controller.Local.Controls;
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
                ConfigurationType = typeof(Configuration),
                ContainerConfigurator = new ContainerConfigurator(this),
                HttpServerPort = 1025
            };

            _controller = new Core.Controller(options);

            // The app is only available from other machines. https://msdn.microsoft.com/en-us/library/windows/apps/Hh780593.aspx
            StoragePathTextBox.Text = StoragePath.StorageRoot;
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

        public async Task<IMotionDetectorAdapter> CreateUIMotionDetectorAdapter(string caption)
        {
            IMotionDetectorAdapter result = null;

            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    var checkBox = new CheckBox
                    {
                        Content = caption
                    };

                    var adapter = new UIMotionDetectorAdapter(checkBox);
                    adapter.Connect();

                    DemoMotionDetectorsPanel.Children.Add(checkBox);

                    result = adapter;
                });

            return result;
        }

        public async Task<IBinaryOutputAdapter> CreateUIBinaryOutputAdapter(string caption)
        {
            IBinaryOutputAdapter result = null;

            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    var checkBox = new CheckBox
                    {
                        IsEnabled = false,
                        Content = caption
                    };

                    var adapter = new UIBinaryOutputAdapter(checkBox);

                    DemoLampsPanel.Children.Add(checkBox);

                    result = adapter;
                });

            return result;
        }

        public async Task<IButtonAdapter> CreateUIButtonAdapter(string caption)
        {
            IButtonAdapter result = null;

            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    var button = new Button { Content = caption };
                    var adapter = new UIButtonAdapter(button);
                    adapter.Connect();

                    DemoButtonPanel.Children.Add(button);

                    result = adapter;
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
