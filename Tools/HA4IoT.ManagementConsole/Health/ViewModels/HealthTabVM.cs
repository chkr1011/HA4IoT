using System;
using System.Windows;
using System.Windows.Input;
using HA4IoT.ManagementConsole.Controller;
using HA4IoT.ManagementConsole.Core;
using HA4IoT.TraceReceiver;

namespace HA4IoT.ManagementConsole.Health.ViewModels
{
    public class HealthTabVM : ViewModelBase
    {
        private readonly ControllerClient _controllerClient;
        private readonly TraceItemReceiverClient _traceItemReceiverClient;

        public HealthTabVM(ControllerClient controllerClient)
        {
            if (controllerClient == null) throw new ArgumentNullException(nameof(controllerClient));

            TraceItems = new SelectableObservableCollection<TraceItem>();

            _controllerClient = controllerClient;

            _traceItemReceiverClient = new TraceItemReceiverClient();
            _traceItemReceiverClient.TraceItemReceived += EnlistTraceItem;
            _traceItemReceiverClient.Start();

            ClearCommand = new DelegateCommand(Clear);

            AutoScroll = new PropertyVM<bool>(true);
            ShowVerboseMessages = new PropertyVM<bool>(true);
            ShowInformations = new PropertyVM<bool>(true);
            ShowWarnings = new PropertyVM<bool>(true);
            ShowErrors  = new PropertyVM<bool>(true);
        }

        public ICommand ClearCommand { get; }

        public SelectableObservableCollection<TraceItem> TraceItems { get; }
        
        public PropertyVM<bool> AutoScroll { get; } 

        public PropertyVM<bool> ShowVerboseMessages { get; }

        public PropertyVM<bool> ShowInformations { get; }

        public PropertyVM<bool> ShowWarnings { get; }
        
        public PropertyVM<bool> ShowErrors { get; }

        private void Clear()
        {
            TraceItems.Clear();
        }

        private void EnlistTraceItem(object sender, TraceItemReceivedEventArgs e)
        {
            if (!e.SenderAddress.ToString().Equals(_controllerClient.Address))
            {
                return;
            }

            if (e.TraceItem.Severity == TraceItemSeverity.Verbose && !ShowVerboseMessages.Value)
            {
                return;
            }

            if (e.TraceItem.Severity == TraceItemSeverity.Info && !ShowInformations.Value)
            {
                return;
            }
            
            if (e.TraceItem.Severity == TraceItemSeverity.Warning && !ShowWarnings.Value)
            {
                return;
            }

            if (e.TraceItem.Severity == TraceItemSeverity.Error && !ShowErrors.Value)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() => TraceItems.Insert(0, e.TraceItem));
        }
    }
}
