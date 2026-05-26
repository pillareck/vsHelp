namespace vsHelp.Core.Models;

public class BackupFile
{
    public string FullPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public string FormattedSize { get; set; } = "0 B";

    public static string FormatSize(long bytes)
    {
        string[] suffix = { "B", "KB", "MB", "GB", "TB" };
        int i;
        double dblSbl = bytes;
        for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
        {
            dblSbl = bytes / 1024.0;
        }

        // Fix for small files
        if (i == 0) return $"{bytes} {suffix[i]}";
        
        // Re-calculate to get precision
        dblSbl = bytes; 
        // Wait, the loop above is slightly flawed for precision. Let's use a cleaner one.
        return FormatSizeClean(i == 0 ? (double)bytes : dblSbl, i);
    }
    
    private static string FormatSizeClean(double bytes, int unitIndex)
    {
        string[] suffix = { "B", "KB", "MB", "GB", "TB" };
        if (unitIndex >= suffix.Length) unitIndex = suffix.Length - 1;
        return string.Format("{0:0.##} {1}", bytes, suffix[unitIndex]);
    }

    // Better implementation
    public static string GetFormattedSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return string.Format("{0:0.##} {1}", len, sizes[order]);
    }
}
