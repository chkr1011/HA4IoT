using System;
using HA4IoT.ManagementConsole.Core;

namespace HA4IoT.ManagementConsole.Home.ViewModels
{
    public abstract class HomeItemVM : ViewModelBase
    {
        protected HomeItemVM(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public string Id { get; private set; }
    }
}
