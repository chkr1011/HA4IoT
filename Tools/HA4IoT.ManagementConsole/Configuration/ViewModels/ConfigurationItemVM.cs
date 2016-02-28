using System;
using HA4IoT.ManagementConsole.Configuration.ViewModels.Settings;
using HA4IoT.ManagementConsole.Core;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels
{
    public abstract class ConfigurationItemVM : ViewModelBase
    {
        protected ConfigurationItemVM(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
            Settings = new SelectableObservableCollection<SettingItemVM>();
        }

        public string Id { get; private set; }

        public SelectableObservableCollection<SettingItemVM> Settings { get; }
    }
}
