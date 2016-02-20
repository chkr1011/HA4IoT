using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HA4IoT.ManagementConsole.Controller;
using HA4IoT.ManagementConsole.Core;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Home.ViewModels
{
    public class HomeTabVM : ViewModelBase
    {
        private readonly ControllerClient _controllerClient = new ControllerClient();

        public HomeTabVM()
        {
            Areas = new SelectableObservableCollection<AreaItemVM>();

            RefreshCommand = new AsyncDelegateCommand(RefreshAsync);
            SaveActuatorSettingsCommand = new AsyncDelegateCommand(SaveActuatorSettingsAsync);
            MoveUpCommand = new DelegateCommand(MoveActuatorUp);
            MoveDownCommand = new DelegateCommand(MoveActuatorDown);

            _controllerClient.Address = "192.168.1.15";
        }

        public SelectableObservableCollection<AreaItemVM> Areas { get; }

        public ICommand RefreshCommand { get; private set; }

        public ICommand SaveActuatorSettingsCommand { get; private set; }
        
        public ICommand MoveUpCommand { get; private set; }

        public ICommand MoveDownCommand { get; private set; }

        private void MoveActuatorUp()
        {
            Areas.SelectedItem.Actuators.MoveItemUp(Areas.SelectedItem.Actuators.SelectedItem);
        }

        private void MoveActuatorDown()
        {
            Areas.SelectedItem.Actuators.MoveItemDown(Areas.SelectedItem.Actuators.SelectedItem);
        }

        private async Task SaveActuatorSettingsAsync()
        {
            try
            {
                UpdateActuatorSortValues();
                foreach (var actuator in Areas.SelectedItem.Actuators)
                {
                    var configruation = JObject.FromObject(actuator.Settings);
                    await _controllerClient.SetActuatorConfiguration(actuator.Id, configruation);
                }

                MessageBox.Show("OK", "Info", MessageBoxButton.OK);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error", MessageBoxButton.OK);
            }
        }

        private async Task RefreshAsync()
        {
            JObject configuration = await _controllerClient.GetConfiguration();
            
            Areas.Clear();
            var parser = new ConfigurationParser();
            foreach (var areaItem in parser.Parse(configuration))
            {
                Areas.Add(areaItem);
            }
        }

        private void UpdateActuatorSortValues()
        {
            int sortValue = 0;
            foreach (var actuator in Areas.SelectedItem.Actuators)
            {
                actuator.Settings.AppSettings.SortValue = sortValue;
                sortValue++;
            }
        }
    }
}
