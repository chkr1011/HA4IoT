using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core;
using HA4IoT.Simulator.Controls;

namespace HA4IoT.Simulator
{
    public sealed partial class MainPage : ILogAdapter
    {
        public MainPage()
        {
            InitializeComponent();
            
            var options = new ControllerOptions
            {
                ConfigurationType = typeof(Configuration),
                ContainerConfigurator = new ContainerConfigurator(this),
                HttpServerPort = 1025
            };

            options.LogAdapters.Add(this);

            var controller = new Controller(options);

            // The app is only available from other machines. https://msdn.microsoft.com/en-us/library/windows/apps/Hh780593.aspx
            StoragePathTextBox.Text = StoragePath.StorageRoot;
            AppPathTextBox.Text = StoragePath.AppRoot;
            ManagementAppPathTextBox.Text = StoragePath.ManagementAppRoot;

            controller.RunAsync();
        }

        private class ContainerConfigurator : IContainerConfigurator
        {
            private readonly MainPage _mainPage;

            public ContainerConfigurator(MainPage mainPage)
            {
                _mainPage = mainPage ?? throw new ArgumentNullException(nameof(mainPage));
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

        public async Task<UIBinaryOutputAdapter> CreateUIBinaryOutputAdapter(string caption)
        {
            UIBinaryOutputAdapter result = null;

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

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = string.Empty;
        }

        public void ProcessLogEntry(LogEntry logEntry)
        {
            var message =
                $"[{logEntry.Id}] [{logEntry.Timestamp}] [{logEntry.Source}] [{logEntry.ThreadId}] [{logEntry.Severity}]: {logEntry.Message}";

            if (!string.IsNullOrEmpty(logEntry.Exception))
            {
                message += Environment.NewLine;
                message += logEntry.Exception;
            }

            LogTextBox.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    LogTextBox.Text += message + Environment.NewLine;

                }).AsTask().Wait();
        }
    }
}
