using vsHelp.Core.Models;

namespace vsHelp.Core.Interfaces;

public interface IArchiveExtractor
{
    Task<ExtractionResult> ExtractAsync(string archivePath, string destinationPath, IProgress<double>? progress = null);
}
