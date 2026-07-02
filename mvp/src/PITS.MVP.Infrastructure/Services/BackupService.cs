using Microsoft.EntityFrameworkCore;
using PITS.MVP.Core.Services;
using PITS.MVP.Infrastructure.Data;

namespace PITS.MVP.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly TripContext _context;

    public BackupService(TripContext context)
    {
        _context = context;
    }

    public async Task<string> BackupAsync(string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);
        var source = _context.Database.GetDbConnection().DataSource;
        if (string.IsNullOrWhiteSpace(source) || source == ":memory:")
            throw new InvalidOperationException("Current database cannot be backed up as a file.");

        await _context.Database.CloseConnectionAsync();
        var backupPath = Path.Combine(destinationDirectory, $"pits-{DateTime.UtcNow:yyyyMMddHHmmss}.db");
        File.Copy(source, backupPath, overwrite: false);
        return backupPath;
    }

    public async Task RestoreAsync(string backupFilePath)
    {
        var target = _context.Database.GetDbConnection().DataSource;
        if (string.IsNullOrWhiteSpace(target) || target == ":memory:")
            throw new InvalidOperationException("Current database cannot be restored as a file.");

        await _context.Database.CloseConnectionAsync();
        File.Copy(backupFilePath, target, overwrite: true);
    }
}
