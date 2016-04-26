using System;
using System.Windows;
using HA4IoT.ManagementConsole.MainWindow.ViewModel;

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
                var mainWindow = new MainWindow.Views.MainWindow();
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
