using System.Globalization;
using System.Text.Json;
using LogHub.Models;
namespace LogHub.Parsing;

public class LogParser
{
    public LogRecord? Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;
        var value = line.Trim();
        return value.StartsWith("{", StringComparison.Ordinal)
            ? ParseJson(value)
            : ParsePipe(value);
    }
    private static LogRecord? ParsePipe(string line)
    {
        var parts = line.Split('|', 3);
        if (parts.Length != 3 || !DateTime.TryParse(parts[0],
                CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
            return null;
        return new LogRecord
        {
            Date = date,
            Severity = parts[1].Trim(),
            Message = parts[2].Trim()
        };
    }
    public LogRecord? ParseJson(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;
        try
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("date", out var dateProperty) ||
                !root.TryGetProperty("level", out var levelProperty) ||
                !root.TryGetProperty("message", out var messageProperty) ||
                dateProperty.ValueKind != JsonValueKind.String ||
                levelProperty.ValueKind != JsonValueKind.String ||
                messageProperty.ValueKind != JsonValueKind.String ||
                !dateProperty.TryGetDateTime(out var date))
                return null;
            return new LogRecord
            {
                Date = date,
                Severity = levelProperty.GetString()!.Trim(),
                Message = messageProperty.GetString()!.Trim()
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
