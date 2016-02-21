using System;
using HA4IoT.ManagementConsole.Core;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels
{
    public abstract class ConfigurationItemVM : ViewModelBase
    {
        protected ConfigurationItemVM(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public string Id { get; private set; }
    }
}
