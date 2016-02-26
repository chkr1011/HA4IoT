using System;
using System.Threading.Tasks;
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

            MoveAreaUpCommand = new DelegateCommand(MoveAreaUp);
            MoveAreaDownCommand = new DelegateCommand(MoveAreaDown);

            MoveActuatorUpCommand = new DelegateCommand(MoveActuatorUp);
            MoveActuatorDownCommand = new DelegateCommand(MoveActuatorDown);
        }

        public SelectableObservableCollection<AreaItemVM> Areas { get; }

        public ICommand RefreshCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }
        
        public ICommand MoveActuatorUpCommand { get; private set; }

        public ICommand MoveActuatorDownCommand { get; private set; }

        public ICommand MoveAreaUpCommand { get; private set; }

        public ICommand MoveAreaDownCommand { get; private set; }

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

        private void MoveAreaUp()
        {
            Areas.MoveItemUp(Areas.SelectedItem);
        }

        private void MoveAreaDown()
        {
            Areas.MoveItemDown(Areas.SelectedItem);
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
                UpdateAreaSortValues();
                UpdateActuatorSortValues();

                foreach (var area in Areas)
                {
                    var configuration = area.SerializeSettings();
                    await _controllerClient.PostAreaConfiguration(area.Id, configuration);
                }

                foreach (var actuator in Areas.SelectedItem.Actuators)
                {
                    var configruation = actuator.SerializeSettings();
                    await _controllerClient.PostActuatorConfiguration(actuator.Id, configruation);
                }

                foreach (var automation in Areas.SelectedItem.Automations)
                {
                    var configuration = JObject.FromObject(automation.Settings);
                    await _controllerClient.PostAutomationConfiguration(automation.Id, configuration);
                }
            }
            catch (Exception exception)
            {
                _unhandledExceptionPresenter.Show(exception);
            }
        }

        private void UpdateAreaSortValues()
        {
            for (int i = 0; i < Areas.Count; i++)
            {
                Areas[i].SortValue = i;
            }
        }

        private void UpdateActuatorSortValues()
        {
            for (int i = 0; i < Areas.SelectedItem.Actuators.Count; i++)
            {
                Areas.SelectedItem.Actuators[i].SortValue = i;
            }
        }
    }
}
