using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Backup;
using HA4IoT.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Backup
{
    [ApiServiceClass(typeof(IBackupService))]
    public class BackupService : ServiceBase, IBackupService
    {
        public event EventHandler<BackupEventArgs> CreatingBackup;
        public event EventHandler<BackupEventArgs> RestoringBackup;

        [ApiMethod]
        public void CreateBackup(IApiCall apiCall)
        {
            var backup = new JObject
            {
                ["Type"] = "HA4IoT.Backup",
                ["Timestamp"] = DateTime.Now.ToString("O"),
                ["Version"] = 1
            };

            var eventArgs = new BackupEventArgs(backup);
            CreatingBackup?.Invoke(this, eventArgs);

            apiCall.Result = backup;
        }

        [ApiMethod]
        public void RestoreBackup(IApiCall apiCall)
        {
            if (apiCall.Parameter.Type != JTokenType.Object)
            {
                throw new NotSupportedException();
            }

            var eventArgs = new BackupEventArgs(apiCall.Parameter);
            RestoringBackup?.Invoke(this, eventArgs);
        }
    }
}
