namespace PITS.MVP.Core.Services;

public interface IBackupService
{
    Task<string> BackupAsync(string destinationDirectory);
    Task RestoreAsync(string backupFilePath);
}
