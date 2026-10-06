using LogHub.Models;

namespace LogHub.Formatting;
public class LogFormatter
{
    private readonly string _format;
    public LogFormatter(string format)
    {
        _format = format;
    }
    public string Format(LogRecord record)
    {
        return _format switch
        {
            "plain" =>
                $"{record.Date:yyyy-MM-dd HH:mm:ss} [{record.Severity}] {record.Message}",
            "json" =>
                "{\"date\":\"" + record.Date.ToString("O") +
                "\",\"level\":\"" + record.Severity.ToLowerInvariant() +
                "\",\"message\":\"" + record.Message.Replace("\"", "\\\"") + "\"}",
            "csv" =>
                $"{record.Date:O};{record.Severity};{record.Message}",
            _ => record.Message
        };
    }
}