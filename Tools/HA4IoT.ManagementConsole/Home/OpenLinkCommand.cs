using System;
using System.Diagnostics;
using System.Windows.Input;

namespace HA4IoT.ManagementConsole.Home
{
    public class OpenLinkCommand : ICommand
    {
        private readonly string _link;

        public OpenLinkCommand(string link)
        {
            if (link == null) throw new ArgumentNullException(nameof(link));

            _link = link;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            using (Process.Start(_link)) { }
        }
    }
}
