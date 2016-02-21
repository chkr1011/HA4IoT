using HA4IoT.ManagementConsole.Core;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels
{
    public class AreaItemVM : ConfigurationItemVM
    {
        public AreaItemVM(string id) : base(id)
        {
            Actuators = new SelectableObservableCollection<ActuatorItemVM>();
            Automations = new SelectableObservableCollection<AutomationItemVM>();
        }

        public string Caption { get; set; }

        public SelectableObservableCollection<ActuatorItemVM> Actuators { get; private set; }

        public SelectableObservableCollection<AutomationItemVM> Automations { get; private set; }
    }
}
