using System;
using System.Diagnostics;
using System.Windows.Input;
using HA4IoT.ManagementConsole.Controller;
using HA4IoT.ManagementConsole.Core;

namespace HA4IoT.ManagementConsole.Home.ViewModels
{
    public class HomeTabVM : ViewModelBase
    {
        private readonly ControllerClient _controllerClient;

        public HomeTabVM(ControllerClient controllerClient)
        {
            if (controllerClient == null) throw new ArgumentNullException(nameof(controllerClient));

            _controllerClient = controllerClient;

            OpenHomepageCommand = new OpenLinkCommand("http://www.HA4IoT.de");
            OpenTwitterCommand = new OpenLinkCommand("https://twitter.com/chkratky");
            OpenRepositoryCommand = new OpenLinkCommand("https://github.com/chkr1011/CK.HomeAutomation");
            OpenDocumentationCommand = new OpenLinkCommand(
                    "https://www.hackster.io/cyborg-titanium-14/home-automation-with-raspberry-pi-2-and-windows-10-iot-784235");

            OpenAppCommand = new DelegateCommand(OpenApp);
        }

        private void OpenApp()
        {
            using (Process.Start($"http://{_controllerClient.Address}/App/index.html")) { }
        }

        public ICommand OpenTwitterCommand { get; }

        public ICommand OpenRepositoryCommand { get; }

        public ICommand OpenHomepageCommand { get; }

        public ICommand OpenDocumentationCommand { get; }

        public ICommand OpenAppCommand { get; }
    }
}
