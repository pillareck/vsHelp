using MySqlConnector;
using vsHelp.Core.Interfaces;
using vsHelp.Core.Models;

namespace vsHelp.Infrastructure.Services;

public class MySqlConnectionService : IDatabaseConnectionService
{
    public async Task<ConnectionTestResult> TestConnectionAsync(DatabaseConnectionSettings settings)
    {
        var result = new ConnectionTestResult();
        try
        {
            using var conn = new MySqlConnection(settings.GetConnectionString(includeDatabase: false));
            await conn.OpenAsync();
            
            result.Success = true;
            result.Message = "Conexão estabelecida com sucesso.";

            // List databases, excluding system ones
            using var cmd = new MySqlCommand("SHOW DATABASES;", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var db = reader.GetString(0);
                if (!IsSystemDatabase(db))
                {
                    result.Databases.Add(db);
                }
            }
            result.Databases.Sort();
        }
        catch (MySqlException ex)
        {
            result.Success = false;
            result.Message = ex.Number switch
            {
                1045 => "Acesso negado: Usuário ou senha incorretos.",
                1042 => "Host não encontrado: Verifique o servidor e porta.",
                _ => $"Erro MySQL ({ex.Number}): {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Erro inesperado: {ex.Message}";
        }
        return result;
    }

    public async Task<bool> CreateDatabaseAsync(DatabaseConnectionSettings settings, string dbName)
    {
        try
        {
            using var conn = new MySqlConnection(settings.GetConnectionString(includeDatabase: false));
            await conn.OpenAsync();
            using var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;", conn);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool IsSystemDatabase(string dbName)
    {
        string[] systems = { "information_schema", "mysql", "performance_schema", "sys" };
        return systems.Contains(dbName.ToLower());
    }
}
