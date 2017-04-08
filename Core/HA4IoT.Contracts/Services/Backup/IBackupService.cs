using System;

namespace HA4IoT.Contracts.Services.Backup
{
    public interface IBackupService : IService
    {
        event EventHandler<BackupEventArgs> CreatingBackup;
        event EventHandler<BackupEventArgs> RestoringBackup;
    }
}
