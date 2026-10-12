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
        return value.StartsWith("{", StringComparison.Ordinal) ? ParseJson(value) : ParsePipe(value);
    }
    private static LogRecord? ParsePipe(string line)
    {
        var parts = line.Split('|', 3);
        if (parts.Length != 3 || !DateTime.TryParse(parts[0], CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var date)) return null;
        return new SimpleLogEntry { Date = date, Severity = parts[1].Trim(), Message = parts[2].Trim() };
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
                !dateProperty.TryGetDateTime(out var date)) return null;
            var level = levelProperty.GetString()!.Trim();
            var message = messageProperty.GetString()!.Trim();
            var kind = "simple";
            if (root.TryGetProperty("kind", out var kindProperty))
            {
                if (kindProperty.ValueKind != JsonValueKind.String) return null;
                kind = kindProperty.GetString()!.ToLowerInvariant();
            }
            if (kind == "simple")
                return new SimpleLogEntry { Date = date, Severity = level, Message = message };
            if (kind == "exception" && root.TryGetProperty("stackTrace", out var trace) &&
                trace.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(trace.GetString()))
                return new ExceptionLogEntry
                    { Date = date, Severity = level, Message = message, StackTrace = trace.GetString()! };
            if (kind == "metric" && root.TryGetProperty("value", out var value) &&
                value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number) && double.IsFinite(number))
                return new MetricLogEntry { Date = date, Severity = level, Message = message, Value = number };
            return null;
        }
        catch (JsonException) { return null; }
    }
}
