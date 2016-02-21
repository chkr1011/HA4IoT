using System;
using System.Windows;
using HA4IoT.ManagementConsole.Configuration.ViewModels;
using HA4IoT.ManagementConsole.Controller;
using HA4IoT.ManagementConsole.Core;
using HA4IoT.ManagementConsole.Properties;

namespace HA4IoT.ManagementConsole.Chrome.ViewModel
{
    public class MainWindowVM : ViewModelBase
    {
        private readonly ControllerClient _controllerClient = new ControllerClient();
        private readonly UnhandledExceptionPresenter _unhandledExceptionPresenter = new UnhandledExceptionPresenter();

        public MainWindowVM()
        {
            _controllerClient.Address = Settings.Default.ControllerAddress;
            _controllerClient.IsWorkingChanged += OnIsWorkingChanged;
            
            ConfigurationTab = new ConfigurationTabVM(_controllerClient, _unhandledExceptionPresenter);
        }

        public bool IsWorking => _controllerClient.IsWorking;

        public ConfigurationTabVM ConfigurationTab { get; private set; }

        public UnhandledExceptionPresenter UnhandledExceptionPresenter => _unhandledExceptionPresenter;

        private void OnIsWorkingChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => OnPropertyChanged("IsWorking")));
        }
    }
}
