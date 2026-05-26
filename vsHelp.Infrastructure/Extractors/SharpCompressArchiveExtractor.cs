using SharpCompress.Archives;
using SharpCompress.Common;
using vsHelp.Core.Interfaces;
using vsHelp.Core.Models;
using System.IO;

namespace vsHelp.Infrastructure.Extractors;

/// <summary>
/// Implementação de extração de arquivos utilizando SharpCompress v0.48.1+.
/// </summary>
public class SharpCompressArchiveExtractor : IArchiveExtractor
{
    public async Task<ExtractionResult> ExtractAsync(string archivePath, string destinationPath, IProgress<double>? progress = null)
    {
        var result = new ExtractionResult();
        try
        {
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            // Na versão 0.48.1+, usamos OpenArchive para manter compatibilidade com o loop de entradas atual.
            // Executamos dentro de Task.Run para garantir que a UI permaneça responsiva.
            await Task.Run(() =>
            {
                using (var archive = ArchiveFactory.OpenArchive(archivePath))
                {
                    var entries = archive.Entries.Where(e => !e.IsDirectory).ToList();
                    int totalEntries = entries.Count;
                    int currentEntry = 0;

                    result.Logs.Add($"[INFO] Abrindo arquivo compactado: {Path.GetFileName(archivePath)}");
                    result.Logs.Add($"[INFO] Total de arquivos para extrair: {totalEntries}");

                    foreach (var entry in entries)
                    {
                        // Extração utilizando as opções atuais da API
                        entry.WriteToDirectory(destinationPath, new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                        
                        string entryKey = entry.Key ?? string.Empty;
                        if (entryKey.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                        {
                            var fullPath = Path.Combine(destinationPath, entryKey);
                            result.SqlFilesFound.Add(fullPath);
                            result.Logs.Add($"[SUCCESS] SQL encontrado: {entryKey}");
                        }
                        
                        currentEntry++;
                        progress?.Report((double)currentEntry / totalEntries * 100);
                    }
                }
            });

            result.Success = true;
            result.ExtractionPath = destinationPath;
            result.Message = "Extração concluída.";
            result.Logs.Add($"[SUCCESS] Todos os arquivos extraídos para {destinationPath}");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Erro na extração SharpCompress: {ex.Message}";
            result.Logs.Add($"[ERROR] {result.Message}");
        }

        return result;
    }
}
