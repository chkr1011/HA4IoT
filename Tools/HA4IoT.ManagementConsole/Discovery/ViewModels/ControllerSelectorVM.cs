using System;
using System.Linq;
using System.Net;
using System.Windows;
using HA4IoT.ManagementConsole.Controller;
using HA4IoT.ManagementConsole.Core;
using HA4IoT.ManagementConsole.Json;

namespace HA4IoT.ManagementConsole.Discovery.ViewModels
{
    public class ControllerSelectorVM : ViewModelBase
    {
        private readonly DiscoveryClient _discoveryClient = new DiscoveryClient();

        public ControllerSelectorVM()
        {
            Controllers = new SelectableObservableCollection<ControllerItemVM>();

            _discoveryClient.ResponseReceived += EnlistController;

            AcceptCommand = new DelegateCommand(Accept, () => Controllers.SelectedItem != null);
            CancelCommand = new DelegateCommand(Cancel);

            Controllers.NotifyCommandIfSelectionChanged(AcceptCommand);
        }

        public event EventHandler ControllerSelected;

        public event EventHandler SelectionCanceled;

        public SelectableObservableCollection<ControllerItemVM> Controllers { get; }

        public DelegateCommand AcceptCommand { get; }

        public DelegateCommand CancelCommand { get; }

        public void StartBroadcasting()
        {
            _discoveryClient.Start();
        }

        private void Accept()
        {
            ControllerSelected?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            SelectionCanceled?.Invoke(this, EventArgs.Empty);    
        }

        private void EnlistController(object sender, DiscoveryResponseReceivedEventArgs e)
        {
            if (e.Response.GetNamedString("Type", string.Empty) != "HA4IoT.DiscoveryResponse")
            {
                return;
            }

            var controllerInformation = e.Response.GetNamedObject("Controller", null);
            if (controllerInformation == null)
            {
                return;
            }

            IPAddress ipAddress = e.EndPoint.Address;
            string caption = controllerInformation.GetNamedString("Caption", string.Empty);
            string description = controllerInformation.GetNamedString("Description", string.Empty);

            EnlistController(ipAddress, caption, description);
        }

        private void EnlistController(IPAddress ipAddress, string caption, string description)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => EnlistController(ipAddress, caption, description));
                return;
            }

            if (Controllers.Any(c => c.IPAddress.Equals(ipAddress)))
            {
                return;
            }

            var controllerItem = new ControllerItemVM(ipAddress, caption, description);
            Controllers.Add(controllerItem);
        }
    }
}
