using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using LogHub.Models;
using LogHub.Visiting;
namespace LogHub.Formatting;
public sealed class LogFormatter : ILogEntryVisitor
{
    private readonly string _format;
    private string _text = string.Empty;
    private static readonly JsonSerializerOptions JsonOptions = new()
        { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public LogFormatter(string format) => _format = format.ToLowerInvariant();
    public string Format(LogRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        record.Accept(this);
        return _text;
    }
    public void Visit(SimpleLogEntry entry) =>
        _text = Render(entry, "simple", string.Empty, new Dictionary<string, object>());
    public void Visit(ExceptionLogEntry entry) =>
        _text = Render(entry, "exception", " | stackTrace=" + SingleLine(entry.StackTrace),
            new Dictionary<string, object> { ["stackTrace"] = entry.StackTrace });
    public void Visit(MetricLogEntry entry) =>
        _text = Render(entry, "metric", " | value=" + entry.Value.ToString("R", CultureInfo.InvariantCulture),
            new Dictionary<string, object> { ["value"] = entry.Value });

    private string Render(LogRecord entry, string kind, string suffix, Dictionary<string, object> details)
    {
        var date = entry.Date.ToString("O", CultureInfo.InvariantCulture);
        var values = new Dictionary<string, object>
        {
            ["date"] = date, ["level"] = entry.Severity.ToLowerInvariant(), ["message"] = entry.Message
        };
        if (details.Count > 0)
        {
            values["kind"] = kind;
            foreach (var pair in details) values[pair.Key] = pair.Value;
        }
        return _format switch
        {
            "plain" => entry.Date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                + $" [{entry.Severity}] {SingleLine(entry.Message)}{suffix}",
            "json" => JsonSerializer.Serialize(values, JsonOptions),
            "csv" => string.Join(';', Csv(date), Csv(entry.Severity), Csv(entry.Message + suffix)),
            _ => SingleLine(entry.Message) + suffix
        };
    }
    private static string SingleLine(string text) =>
        text.Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);
    private static string Csv(string text) => text.IndexOfAny(new[] { ';', '"', '\r', '\n' }) < 0
        ? text : "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}
