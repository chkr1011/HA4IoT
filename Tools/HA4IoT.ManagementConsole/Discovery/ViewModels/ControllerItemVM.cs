using System;
using System.Net;
using HA4IoT.ManagementConsole.Core;

namespace HA4IoT.ManagementConsole.Discovery.ViewModels
{
    public class ControllerItemVM : ViewModelBase
    {
        public ControllerItemVM(IPAddress ipAddress, string caption, string description)
        {
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
            if (caption == null) throw new ArgumentNullException(nameof(caption));
            if (description == null) throw new ArgumentNullException(nameof(description));

            IPAddress = ipAddress;
            Caption = caption;
            Description = description;
        }

        public IPAddress IPAddress { get; private set; }

        public string Caption { get; private set; }

        public string Description { get; private set; }
    }
}
