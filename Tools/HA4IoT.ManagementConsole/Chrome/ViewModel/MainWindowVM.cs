using System;
using System.Windows;
using HA4IoT.ManagementConsole.Configuration.ViewModels;
using HA4IoT.ManagementConsole.Controller;
using HA4IoT.ManagementConsole.Core;
using HA4IoT.ManagementConsole.Discovery.ViewModels;
using HA4IoT.ManagementConsole.Home.ViewModels;

namespace HA4IoT.ManagementConsole.Chrome.ViewModel
{
    public class MainWindowVM : ViewModelBase
    {
        private readonly ControllerClient _controllerClient = new ControllerClient();

        private readonly UnhandledExceptionPresenter _unhandledExceptionPresenter = new UnhandledExceptionPresenter();
        private readonly ControllerSelectorVM _controllerSelector = new ControllerSelectorVM();

        private ViewModelBase _dialog;

        public MainWindowVM()
        {
            ControllerAddress = new PropertyVM<string>("N/A");

            _controllerClient.IsWorkingChanged += OnIsWorkingChanged;

            _controllerSelector.StartBroadcasting();
            _controllerSelector.ControllerSelected += async (s, e) =>
            {
                _controllerClient.Address = _controllerSelector.SelectedControllerAddress;
                ControllerAddress.Value = _controllerClient.Address;

                await ConfigurationTab.RefreshAsync();
                Dialog = null;
            };

            _controllerSelector.SelectionCanceled += (s, e) =>
            {
                Application.Current.Shutdown();
            };

            HomeTab = new HomeTabVM(_controllerClient);
            ConfigurationTab = new ConfigurationTabVM(_controllerClient, _unhandledExceptionPresenter);

            Dialog = _controllerSelector;
        }

        public bool IsWorking => _controllerClient.IsWorking;

        public HomeTabVM HomeTab { get; }

        public ConfigurationTabVM ConfigurationTab { get; }

        public PropertyVM<string> ControllerAddress { get; }

        public UnhandledExceptionPresenter UnhandledExceptionPresenter => _unhandledExceptionPresenter;

        public ViewModelBase Dialog
        {
            get { return _dialog; }
            set
            {
                _dialog = value;
                OnPropertyChangedFromCaller();
                OnPropertyChanged("IsDialogShown");
            }
        }

        public bool IsDialogShown => _dialog != null;

        private void OnIsWorkingChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => OnPropertyChanged("IsWorking")));
        }
    }
}
