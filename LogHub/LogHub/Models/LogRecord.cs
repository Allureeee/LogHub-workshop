using LogHub.Visiting;
namespace LogHub.Models;
public abstract class LogRecord
{
    public DateTime Date { get; init; }
    public string Severity { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public abstract void Accept(ILogEntryVisitor visitor);
}
