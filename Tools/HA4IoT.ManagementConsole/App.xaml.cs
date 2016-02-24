using System;
using System.Windows;
using HA4IoT.ManagementConsole.Chrome.ViewModel;
using HA4IoT.ManagementConsole.Chrome.Views;

namespace HA4IoT.ManagementConsole
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs startupEventArgs)
        {
            base.OnStartup(startupEventArgs);

            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            try
            {
                var viewModel = new MainWindowVM();
                var mainWindow = new MainWindow();
                mainWindow.DataContext = viewModel;

                mainWindow.Show();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Startup failed - HA4IoT Management Console", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
    }
}
