using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HA4IoT.ManagementConsole.Chrome.ViewModel;
using HA4IoT.ManagementConsole.Controller;
using HA4IoT.ManagementConsole.Core;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels
{
    public class ConfigurationTabVM : ViewModelBase
    {
        private readonly ControllerClient _controllerClient;
        private readonly UnhandledExceptionPresenter _unhandledExceptionPresenter;

        public ConfigurationTabVM(ControllerClient controllerClient, UnhandledExceptionPresenter unhandledExceptionPresenter)
        {
            if (controllerClient == null) throw new ArgumentNullException(nameof(controllerClient));
            if (unhandledExceptionPresenter == null) throw new ArgumentNullException(nameof(unhandledExceptionPresenter));

            _controllerClient = controllerClient;
            _unhandledExceptionPresenter = unhandledExceptionPresenter;

            Areas = new SelectableObservableCollection<AreaItemVM>();

            RefreshCommand = new AsyncDelegateCommand(RefreshAsync);
            SaveCommand = new AsyncDelegateCommand(SaveAsync);
            MoveUpCommand = new DelegateCommand(MoveActuatorUp);
            MoveDownCommand = new DelegateCommand(MoveActuatorDown);
        }

        public SelectableObservableCollection<AreaItemVM> Areas { get; }

        public ICommand RefreshCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }
        
        public ICommand MoveUpCommand { get; private set; }

        public ICommand MoveDownCommand { get; private set; }

        public async Task RefreshAsync()
        {
            JObject configuration;
            try
            {
                configuration = await _controllerClient.GetConfiguration();
            }
            catch (Exception exception)
            {
                _unhandledExceptionPresenter.Show(exception);
                return;
            }
            
            Areas.Clear();
            var parser = new ConfigurationParser();
            foreach (var areaItem in parser.Parse(configuration))
            {
                Areas.Add(areaItem);
            }
        }

        private void MoveActuatorUp()
        {
            Areas.SelectedItem.Actuators.MoveItemUp(Areas.SelectedItem.Actuators.SelectedItem);
        }

        private void MoveActuatorDown()
        {
            Areas.SelectedItem.Actuators.MoveItemDown(Areas.SelectedItem.Actuators.SelectedItem);
        }

        private async Task SaveAsync()
        {
            try
            {
                UpdateActuatorSortValues();
                foreach (var actuator in Areas.SelectedItem.Actuators)
                {
                    var configruation = actuator.SerializeSettings();
                    await _controllerClient.SetActuatorConfiguration(actuator.Id, configruation);
                }

                foreach (var automation in Areas.SelectedItem.Automations)
                {
                    var configuration = JObject.FromObject(automation.Settings);
                    await _controllerClient.SetAutomationConfiguration(automation.Id, configuration);
                }
            }
            catch (Exception exception)
            {
                _unhandledExceptionPresenter.Show(exception);
            }
        }

        private void UpdateActuatorSortValues()
        {
            int sortValue = 0;
            foreach (var actuator in Areas.SelectedItem.Actuators)
            {
                actuator.SortValue = sortValue;
                sortValue++;
            }
        }
    }
}
