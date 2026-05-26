using MySqlConnector;
using vsHelp.Core.Interfaces;
using vsHelp.Core.Models;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace vsHelp.Infrastructure.Services;

public class MySqlRestoreService : IDatabaseRestoreService
{
    public async Task<RestoreResult> RestoreAsync(string sqlFilePath, DatabaseConnectionSettings settings, IProgress<double>? progress = null)
    {
        var result = new RestoreResult();
        try
        {
            if (!File.Exists(sqlFilePath))
                return RestoreResult.Failed("Arquivo SQL não encontrado para restauração.");

            result.Logs.Add($"[INFO] Conectando ao servidor MySQL: {settings.Server}:{settings.Port}");

            // Step 1: Create Database if not exists
            using (var adminConn = new MySqlConnection(settings.GetConnectionString(includeDatabase: false)))
            {
                await adminConn.OpenAsync();
                result.Logs.Add($"[INFO] Verificando banco de dados: {settings.DatabaseName}");
                
                using var createDbCmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{settings.DatabaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;", adminConn);
                await createDbCmd.ExecuteNonQueryAsync();
                result.Logs.Add($"[SUCCESS] Banco de dados preparado.");
            }

            // Step 2: Restore Content
            using (var conn = new MySqlConnection(settings.GetConnectionString(includeDatabase: true)))
            {
                await conn.OpenAsync();
                result.Logs.Add($"[INFO] Lendo script SQL e executando comandos...");

                // Manual split by semicolon (Basic approach, can be improved for DELIMITERS)
                var sqlContent = await File.ReadAllTextAsync(sqlFilePath, Encoding.UTF8);
                
                // Remove comments to avoid issues with semicolon inside comments
                var sqlWithoutComments = Regex.Replace(sqlContent, @"(--.*)|(/\*[\s\S]*?\*/)", "");
                
                var statements = sqlWithoutComments.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                int total = statements.Length;
                int current = 0;

                foreach (var statement in statements)
                {
                    if (string.IsNullOrWhiteSpace(statement)) continue;

                    using var cmd = new MySqlCommand(statement, conn);
                    await cmd.ExecuteNonQueryAsync();
                    
                    current++;
                    progress?.Report((double)current / total * 100);
                }
                
                result.Logs.Add($"[SUCCESS] {current} comandos SQL executados.");
            }

            return RestoreResult.Succeeded("Restauração MySQL concluída com êxito.");
        }
        catch (MySqlException ex)
        {
            result.Logs.Add($"[ERROR] Erro no MySQL ({ex.Number}): {ex.Message}");
            return RestoreResult.Failed($"Erro de banco de dados: {ex.Message}");
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[ERROR] Erro crítico na restauração: {ex.Message}");
            return RestoreResult.Failed($"Erro inesperado: {ex.Message}");
        }
    }
}
