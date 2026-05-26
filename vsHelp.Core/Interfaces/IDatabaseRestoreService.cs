using vsHelp.Core.Models;

namespace vsHelp.Core.Interfaces;

public interface IDatabaseRestoreService
{
    Task<RestoreResult> RestoreAsync(string sqlFilePath, DatabaseConnectionSettings settings, IProgress<double>? progress = null);
}
