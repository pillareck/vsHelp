using vsHelp.Core.Models;

namespace vsHelp.Core.Interfaces;

public interface IDatabaseConnectionService
{
    Task<ConnectionTestResult> TestConnectionAsync(DatabaseConnectionSettings settings);
    Task<bool> CreateDatabaseAsync(DatabaseConnectionSettings settings, string dbName);
}
