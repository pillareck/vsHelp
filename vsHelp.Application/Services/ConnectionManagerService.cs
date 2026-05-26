using vsHelp.Core.Interfaces;
using vsHelp.Core.Models;
using vsHelp.Infrastructure.Services;

namespace vsHelp.Application.Services;

public class ConnectionManagerService
{
    private readonly IDatabaseConnectionService _connectionService;

    public ConnectionManagerService()
    {
        _connectionService = new MySqlConnectionService();
    }

    public Task<ConnectionTestResult> ConnectAndListDatabasesAsync(DatabaseConnectionSettings settings)
    {
        return _connectionService.TestConnectionAsync(settings);
    }

    public Task<bool> CreateDatabaseAsync(DatabaseConnectionSettings settings, string dbName)
    {
        return _connectionService.CreateDatabaseAsync(settings, dbName);
    }
}
