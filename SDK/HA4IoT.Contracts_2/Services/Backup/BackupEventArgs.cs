using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Services.Backup
{
    public class BackupEventArgs : EventArgs
    {
        public BackupEventArgs(JObject backup)
        {
            if (backup == null) throw new ArgumentNullException(nameof(backup));

            Backup = backup;
        }

        public JObject Backup { get; }
    }
}
