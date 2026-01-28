using System.Text;

namespace CafePos.Presentation.Services;

public static class CsvExporter
{
    public static string Export<T>(IEnumerable<T> rows, Func<T, string> formatter, string fileNamePrefix)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "exports");
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var sb = new StringBuilder();
        foreach (var row in rows)
        {
            sb.AppendLine(formatter(row));
        }
        File.WriteAllText(filePath, sb.ToString());
        return filePath;
    }
}
