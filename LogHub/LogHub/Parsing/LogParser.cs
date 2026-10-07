using System;
using System.Globalization;
using System.Text.Json;
using LogHub.Models;

namespace LogHub.Parsing;
public class LogParser
{
    public LogRecord? Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }
        var trimmedLine = line.Trim();
        if (trimmedLine.StartsWith("{", StringComparison.Ordinal))
        {
            return ParseJson(trimmedLine);
        }
        return ParsePipe(trimmedLine);
    }
    private static LogRecord? ParsePipe(string line)
    {
        var parts = line.Split('|', 3);
        if (parts.Length != 3)
        {
            return null;
        }
        if (!DateTime.TryParse(
                parts[0],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            return null;
        }
        return new LogRecord
        {
            Date = date,
            Severity = parts[1].Trim(),
            Message = parts[2].Trim()
        };
    }
    private static LogRecord? ParseJson(string line)
    {
        try
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            if (!root.TryGetProperty("date", out var dateProperty) ||
                !root.TryGetProperty("level", out var levelProperty) ||
                !root.TryGetProperty("message", out var messageProperty))
            {
                return null;
            }
            if (!dateProperty.TryGetDateTime(out var date))
            {
                return null;
            }
            var severity = levelProperty.GetString();
            var message = messageProperty.GetString();
            if (severity is null || message is null)
            {
                return null;
            }
            return new LogRecord
            {
                Date = date,
                Severity = severity.Trim(),
                Message = message.Trim()
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }
}