using HA4IoT.Contracts.Services;
using System;

namespace HA4IoT.Contracts.Backup
{
    public interface IBackupService : IService
    {
        event EventHandler<BackupEventArgs> CreatingBackup;
        event EventHandler<BackupEventArgs> RestoringBackup;
    }
}
